using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using Castle.Facilities.AutoTx;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Registration;
using Castle.Services.Transaction;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NLog;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	public class ManagingMultipleDatabases
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private WindsorContainer c;

		[SetUp]
		public void SetUp()
		{
			c = GetWindsorContainer();
		}

		[Test]
		public void ConnetionStringShouldContain_memory_DefinedInTheSecondDatabaseSetup()
		{
			// given
			var component = c.Resolve<RepClass>();

			// then
			var connectionstring = component.GetConnectionStringForGivenDatabase(SecondExampleInstaller.Key);
			Assert.That(connectionstring.Contains("memory"), 
				"Err, could not find correct session for given database configuration though SessionFactoryKey {0}.", SecondExampleInstaller.Key);
		}

		private static WindsorContainer GetWindsorContainer()
		{
			var c = new WindsorContainer();

			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());
			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<SecondExampleInstaller>());

			c.AddFacility<FactorySupportFacility>();
			c.AddFacility<AutoTxFacility>();
			c.AddFacility<NHibernateFacility>();

			c.Register(Component.For<RepClass>());

			return c;
		}
	}

	internal class SecondExampleInstaller : INHibernateInstaller
	{
		public const string Key = "sf.second";
		private readonly Maybe<IInterceptor> interceptor;

		public SecondExampleInstaller()
		{
			interceptor = Maybe.None<IInterceptor>();
		}

		public SecondExampleInstaller(IInterceptor interceptor)
		{
			this.interceptor = Maybe.Some(interceptor);
		}

		public Maybe<IInterceptor> Interceptor
		{
			get { return interceptor; }
		}

		public bool IsDefault
		{
			get { return false; }
		}

		public string SessionFactoryKey
		{
			get { return Key; }
		}

		public FluentConfiguration BuildFluent()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["secondtest"];
			Contract.Assume(connectionString != null, "please set the \"secondtest\" connection string in app.config");

			return Fluently.Configure()
				.Database(SQLiteConfiguration.Standard
							.ConnectionString(connectionString.ConnectionString))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ThingMap>());
		}

		public void Registered(ISessionFactory factory)
		{
			new SchemaExport(BuildFluent().BuildConfiguration()).Execute(false, false, true);
		}
	}

	public class RepClass
	{
		private readonly Func<string, ISession> _getSession;

		public RepClass(Func<string, ISession> getSession)
		{
			if (getSession == null) throw new ArgumentNullException("getSession");
			_getSession = getSession;
		}

		[Transaction]
		public virtual string GetConnectionStringForGivenDatabase(string sessionFactoryKey)
		{
			return _getSession(sessionFactoryKey).Connection.ConnectionString;
		}
	}
}