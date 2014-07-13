// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Castle.Transactions.Helpers;

namespace Castle.Facilities.NHibernate
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;

	using Castle.Core.Logging;
	using Castle.Facilities.AutoTx;
	using Castle.Facilities.FactorySupport;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.Transactions;

	using global::NHibernate;
	using global::NHibernate.Cfg;

	using ILoggerFactory = Castle.Core.Logging.ILoggerFactory;

	///<summary>
	///	Easy NHibernate integration with declarative transactions 
	///	using Castle Transaction Services and .Net System.Transactions.
	///	Integrate Transactional NTFS with NHibernate and database transactions, 
	///	or choose methods to fork dependent transactions for to run your transaction 
	///	constituents in parallel. The NHibernate Facility is configured 
	///	using FluentNHibernate
	///</summary>
	public class NHibernateFacility : AbstractFacility
	{
		private ILogger logger = NullLogger.Instance;
		private DefaultSessionLifeStyleOption defaultLifeStyle;
		private FlushMode flushMode;

		/// <summary>
		/// 	The suffix on the name of the component that has a lifestyle of Per Transaction.
		/// </summary>
		public const string SessionPerTxSuffix = "-session";

		///<summary>
		///	The suffix on the name of the ISession/component that has a lifestyle of Per Web Request.
		///</summary>
		public const string SessionPWRSuffix = "-session-pwr";

		/// <summary>
		/// 	The suffix on the name of the ISession/component that has a transient lifestyle.
		/// </summary>
		public const string SessionTransientSuffix = "-session-transient";

		/// <summary>
		/// 	The suffix of the session manager component.
		/// </summary>
		public const string SessionManagerSuffix = "-manager";

		/// <summary>
		/// 	The infix (fackey-[here]-session) of stateless session in the naming of components
		/// 	in Windsor.
		/// </summary>
		public const string SessionStatelessInfix = "-stateless";

		/// <summary>
		/// 	Instantiates a new NHibernateFacility with the default options, session per transaction
		/// 	and automatic flush mode.
		/// </summary>
		public NHibernateFacility() : this(DefaultSessionLifeStyleOption.SessionPerTransaction, FlushMode.Auto)
		{
		}

		/// <summary>
		/// 	Instantiates a new NHibernateFacility with a given lifestyle option and automatic flush mode.
		/// </summary>
		/// <param name = "defaultLifeStyle">The Session flush mode.</param>
		public NHibernateFacility(DefaultSessionLifeStyleOption defaultLifeStyle) : this(defaultLifeStyle, FlushMode.Auto)
		{
		}

		/// <summary>
		/// 	Instantiates a new NHibernateFacility with the default options.
		/// </summary>
		/// <param name = "defaultLifeStyle">The </param>
		/// <param name = "flushMode">The session flush mode</param>
		public NHibernateFacility(DefaultSessionLifeStyleOption defaultLifeStyle, FlushMode flushMode)
		{
			this.defaultLifeStyle = defaultLifeStyle;
			this.flushMode = flushMode;
		}

		/// <summary>
		/// 	Gets or sets the default session life style option.
		/// </summary>
		public DefaultSessionLifeStyleOption DefaultLifeStyle
		{
			get { return defaultLifeStyle; }
			set { defaultLifeStyle = value; }
		}

		/// <summary>
		/// 	Gets or sets the default nhibernate session flush mode. This
		/// 	mode does not apply to stateless sessions.
		/// </summary>
		public FlushMode FlushMode
		{
			get { return flushMode; }
			set { flushMode = value; }
		}

		///<summary>
		///	Initialize, override. Registers everything relating to NHibernate in the container, including:
		///	<see cref = "ISessionFactory" />, <see cref = "ISessionManager" />, <see cref = "Func{TResult}" />, <see
		/// 	cref = "Configuration" />,
		///	<see cref = "ISession" />, <see cref = "IStatelessSession" />.
		///</summary>
		///<remarks>
		///	Requires <see cref = "TypedFactoryFacility" /> and <see cref = "FactorySupportFacility" /> which will be registered by this
		///	facility if there are none already registered.
		///</remarks>
		///<exception cref = "FacilityException">
		///	If any of:
		///	<list type = "bullet">
		///		<item>You haven't added <see cref = "AutoTxFacility" />.</item>
		///		<item>no <see cref = "INHibernateInstaller" /> components registered</item>
		///		<item>one or many of the <see cref = "INHibernateInstaller" /> components had a null or empty session factory key returned</item>
		///		<item>zero or more than one of the <see cref = "INHibernateInstaller" /> components had <see
		/// 	cref = "INHibernateInstaller.IsDefault" /> returned as true</item>
		///		<item>duplicate <see cref = "INHibernateInstaller.SessionFactoryKey" />s registered</item>
		///	</list>
		///</exception>
		[ContractVerification(false)] // interactive bits don't have contracts
		protected override void Init()
		{
			// check we have a logger factory
			if (Kernel.HasComponent(typeof(ILoggerFactory)))
			{
				// get logger factory
				var loggerFactory = Kernel.Resolve<ILoggerFactory>();
				// get logger
				logger = loggerFactory.Create(typeof(AutoTxFacility));
			}

			if (logger.IsDebugEnabled)
				logger.Debug("initializing NHibernateFacility");

			var installers = Kernel.ResolveAll<INHibernateInstaller>();

			Contract.Assume(installers != null, "ResolveAll shouldn't return null");

			if (installers.Length == 0)
				throw new FacilityException("no INHibernateInstaller-s registered.");

			var count = installers.Count(x => x.IsDefault);
			if (count == 0 || count > 1)
				throw new FacilityException("no INHibernateInstaller has IsDefault = true or many have specified it");

			if (installers.Any(x => string.IsNullOrEmpty(x.SessionFactoryKey)))
				throw new FacilityException("all session factory keys must be non null and non empty strings");

			VerifyLegacyInterceptors();

			AssertHasFacility<AutoTxFacility>();

			AddFacility<FactorySupportFacility>();
			AddFacility<TypedFactoryFacility>();

			if (logger.IsDebugEnabled)
				logger.Debug("registering facility components");

			var added = new HashSet<string>();

			var installed = installers
				.Select(x => new
				{
					Config = x.Config,
					Instance = x
				})
				.Select(x => new Data { Config = x.Config, Instance = x.Instance, Factory = x.Config.BuildSessionFactory() })
				.OrderByDescending(x => x.Instance.IsDefault)
				.Do(x =>
				{
					if (!added.Add(x.Instance.SessionFactoryKey))
						throw new FacilityException(
							string.Format(
								"Duplicate session factory keys '{0}' added. Verify that your INHibernateInstaller instances are not named the same.",
								x.Instance.SessionFactoryKey));
				})
				.Do(x => Kernel.Register(
					Component.For<Configuration>()
						.Instance(x.Config)
						.LifeStyle.Singleton
						.Named(x.Instance.SessionFactoryKey + "-cfg"),
					Component.For<ISessionFactory>()
						.Instance(x.Factory)
						.LifeStyle.Singleton
						.Named(x.Instance.SessionFactoryKey),
					RegisterSession(x, 0),
					RegisterSession(x, 1),
					RegisterSession(x, 2),
					RegisterStatelessSession(x, 0),
					RegisterStatelessSession(x, 1),
					RegisterStatelessSession(x, 2),
					Component.For<ISessionManager>().Instance(new SessionManager(() =>
					{
						var factory = Kernel.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey);
						var s = x.Instance.Interceptor.Do(y => factory.OpenSession(y)).OrDefault(factory.OpenSession());
						s.FlushMode = flushMode;
						return s;
					}, Kernel.Resolve<ITransactionManager>()))
						.Named(x.Instance.SessionFactoryKey + SessionManagerSuffix)
						.LifeStyle.Singleton))
				.ToList();

			if (logger.IsDebugEnabled)
				logger.Debug("notifying the nhibernate installers that they have been configured");

			installed.Run(x => x.Instance.Registered(x.Factory));

			if (logger.IsDebugEnabled)
				logger.Debug("Initialized NHibernateFacility");
		}

		private IRegistration RegisterStatelessSession(Data x, uint index)
		{
			Contract.Requires(index < 3,
							  "there are only three supported lifestyles; per transaction, per web request and transient");
			Contract.Requires(x != null);
			Contract.Ensures(Contract.Result<IRegistration>() != null);

			var registration = Component.For<IStatelessSession>()
				.UsingFactoryMethod(k => k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey).OpenStatelessSession());

			return GetLifeStyle(registration, index, x.Instance.SessionFactoryKey + SessionStatelessInfix);
		}

		private IRegistration RegisterSession(Data x, uint index)
		{
			Contract.Requires(index < 3,
							  "there are only three supported lifestyles; per transaction, per web request and transient");
			Contract.Requires(x != null);
			Contract.Ensures(Contract.Result<IRegistration>() != null);

			return GetLifeStyle(
				Component.For<ISession>()
					.UsingFactoryMethod((k, c) =>
					{
						var factory = k.Resolve<ISessionFactory>(x.Instance.SessionFactoryKey);
						var s = x.Instance.Interceptor.Do(y => factory.OpenSession(y)).OrDefault(factory.OpenSession());
						s.FlushMode = flushMode;
						logger.DebugFormat("resolved session component named '{0}'", c.Handler.ComponentModel.Name);
						return s;
					}), index, x.Instance.SessionFactoryKey);
		}

		private ComponentRegistration<T> GetLifeStyle<T>(ComponentRegistration<T> registration, uint index, string baseName)
			where T : class
		{
			Contract.Requires(index < 3,
							  "there are only three supported lifestyles; per transaction, per web request and transient");
			Contract.Ensures(Contract.Result<ComponentRegistration<T>>() != null);

			switch (defaultLifeStyle)
			{
				case DefaultSessionLifeStyleOption.SessionPerTransaction:
					if (index == 0)
						return registration.Named(baseName + SessionPerTxSuffix).LifeStyle.PerTopTransaction();
					if (index == 1)
						return registration.Named(baseName + SessionPWRSuffix).LifeStyle.PerWebRequest;
					if (index == 2)
						return registration.Named(baseName + SessionTransientSuffix).LifeStyle.Transient;
					break;
				case DefaultSessionLifeStyleOption.SessionPerWebRequest:
					if (index == 0)
						return registration.Named(baseName + SessionPWRSuffix).LifeStyle.PerWebRequest;
					if (index == 1)
						return registration.Named(baseName + SessionPerTxSuffix).LifeStyle.PerTopTransaction();
					if (index == 2)
						return registration.Named(baseName + SessionTransientSuffix).LifeStyle.Transient;
					break;
				case DefaultSessionLifeStyleOption.SessionTransient:
					if (index == 0)
						return registration.Named(baseName + SessionTransientSuffix).LifeStyle.Transient;
					if (index == 1)
						return registration.Named(baseName + SessionPerTxSuffix).LifeStyle.PerTopTransaction();
					if (index == 2)
						return registration.Named(baseName + SessionPWRSuffix).LifeStyle.PerWebRequest;
					break;
				default:
					throw new FacilityException("Unknown default life style - please file a bug report");
			}
			throw new FacilityException("Invalid index passed to GetLifeStyle<T> - please file a bug report");
		}

		private class Data
		{
			public INHibernateInstaller Instance;
			public Configuration Config;
			public ISessionFactory Factory;
		}

		private void AssertHasFacility<T>()
		{
			var facilities = Kernel.GetFacilities();

			Contract.Assume(facilities != null, "GetFacilities shouldn't return null");

			var type = typeof(T);
			if (!facilities.Select(x => x.ToString()).Contains(type.ToString()))
				throw new FacilityException(
					string.Format(
						"The NHibernateFacility is dependent on the '{0}' facility. "
						+ "Please add the facility by writing \"container.AddFacility<{1}>()\" or adding it to your config file.",
						type, type.Name));
		}

		private void VerifyLegacyInterceptors()
		{
			if (Kernel.HasComponent("nhibernate.session.interceptor"))
				logger.Warn("component with key \"nhibernate.session.interceptor\" found! this interceptor will not be used.");
		}

		// even though this is O(3n), n ~= 3, so we don't mind it
		private void AddFacility<T>() where T : IFacility, new()
		{
			var facilities = Kernel.GetFacilities();

			Contract.Assume(facilities != null, "GetFacilities shouldn't return null");

			if (facilities.Select(x => x.ToString()).Contains(typeof(T).ToString())) return;

			logger.InfoFormat(
				"facility '{0}' wasn't found in kernel, adding it, because it's a requirement for NHibernateFacility",
				typeof(T));

			Kernel.AddFacility<T>();
		}
	}
}