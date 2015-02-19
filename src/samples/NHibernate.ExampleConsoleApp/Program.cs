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

namespace NHibernate.ExampleConsoleApp
{
	using System;

	using Castle.Facilities.AutoTx;
	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.NHibernate;
	using Castle.MicroKernel.Registration;
	using Castle.Transactions;
	using Castle.Windsor;

	using FluentNHibernate.Cfg;
	using FluentNHibernate.Cfg.Db;
	using FluentNHibernate.Mapping;

	using NHibernate.Cfg;
	using NHibernate.Tool.hbm2ddl;

	using NLog;

	using Topshelf;
	using Castle.Facilities.Logging;

	internal class Program
	{
		private static void Main(string[] args)
		{
			// log4net.ConfigBasicConfigurator.Configure(); // supporting topshelf & its logging to log4net - enable if you want detailed logs
			HostFactory.Run(cfg =>
			{
				cfg.Service<Program>(s =>
				{
					s.ConstructUsing(() => new Program());
					s.WhenStarted(p => p.Start());
					s.WhenStopped(p => p.Stop());
				});

				cfg.RunAsNetworkService();

				cfg.SetDescription("A service logging when it starts and stops to SQLite.");
				cfg.SetDisplayName("NHibernate Logger");
				cfg.SetServiceName("nhibLogger");
			});
		}

		private IWindsorContainer container;

		private void Start()
		{
			container = new WindsorContainer();
			container.AddFacility<LoggingFacility>(f => f.UseNLog());

			container
				.AddFacility<AutoTxFacility>()
				.Register(
					Component.For<INHibernateInstaller>().ImplementedBy<NHibInstaller>().LifeStyle.Singleton,
					Component.For<Logger>().LifeStyle.Singleton)
				.AddFacility<NHibernateFacility>();

			using (var scope = new ResolveScope<Logger>(container))
			{
				using (var up = new ResolveScope<Configuration>(container))
					new SchemaUpdate(up.Service).Execute(false, true);

				Console.WriteLine("Current log contents:");
				Console.WriteLine("[utc date] - [text]");
				Console.WriteLine("-------------------");
				scope.Service.ReadLog(Console.WriteLine);
				scope.Service.WriteToLog(string.Format("{0} - Started", DateTime.UtcNow));
			}
		}

		private void Stop()
		{
			using (var scope = new ResolveScope<Logger>(container))
				scope.Service.WriteToLog(string.Format("{0} - Stopped", DateTime.UtcNow));

			foreach (var target in LogManager.Configuration.AllTargets)
				target.Dispose();

			container.Dispose();
			container = null;
		}
	}

	public class Logger
	{
		private readonly Func<ISession> getSession;

		public Logger(Func<ISession> getSession)
		{
			this.getSession = getSession;
		}

		[Transaction]
		public virtual void WriteToLog(string text)
		{
			using (var s = getSession())
				s.Save(new LogLine(text));
		}

		[Transaction]
		public virtual void ReadLog(Action<string> reader)
		{
			using (var s = getSession())
				foreach (var line in s.CreateCriteria<LogLine>().List<LogLine>())
					reader(line.Line);
		}
	}

	[Serializable]
	public class LogLine
	{
		/// <summary>
		/// Gets the log-line.
		/// </summary>
		public virtual string Line { get; protected set; }

		/// <summary>
		/// Gets the ID of the line in the log.
		/// </summary>
		public virtual Guid Id { get; protected set; }

		/// <summary> for serialization </summary>
		[Obsolete("for serialization")]
		protected LogLine()
		{
		}

		public LogLine(string line)
		{
			Line = line;
		}
	}

	public class LogLineMap 
		: ClassMap<LogLine>
	{
		public LogLineMap()
		{
			Id(x => x.Id).GeneratedBy.GuidComb();
			Map(x => x.Line);
		}
	}

	internal class NHibInstaller : INHibernateInstaller
	{
		public bool IsDefault
		{
			get { return true; }
		}

		public string SessionFactoryKey
		{
			get { return "def"; }
		}

		public Maybe<IInterceptor> Interceptor
		{
			get { return Maybe.None<IInterceptor>(); }
		}

		public Configuration Config
		{
			get
			{
				return Fluently.Configure()
					.Database(SQLiteConfiguration.Standard.UsingFile("DataStore.db"))
					.Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHibInstaller>()).BuildConfiguration();
			}
		}

		public void Registered(ISessionFactory factory)
		{
		}

		public Configuration Deserialize()
		{
			return null;
		}

		public void Serialize(Configuration configuration)
		{
		}

		public void AfterDeserialize(Configuration configuration)
		{
		}
	}
}