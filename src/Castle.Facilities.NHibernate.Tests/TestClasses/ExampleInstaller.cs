using System.Configuration;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	internal class ExampleInstaller : INHibernateInstaller
	{
		private readonly Maybe<IInterceptor> _Interceptor;

		public ExampleInstaller()
		{
			_Interceptor = Maybe.None<IInterceptor>();
		}

		public ExampleInstaller(IInterceptor interceptor)
		{
			_Interceptor = Maybe.Some(interceptor);
		}

		public Maybe<IInterceptor> Interceptor
		{
			get { return _Interceptor; }
		}

		public bool IsDefault
		{
			get { return true; }
		}

		public string SessionFactoryKey
		{
			get { return "sf.default"; }
		}

		public FluentConfiguration BuildFluent()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["test"];
			Contract.Assume(connectionString != null, "please set the \"test\" connection string in app.config");

			return Fluently.Configure()
				.Database(MsSqlConfiguration.MsSql2008.DefaultSchema("dbo")
							.ConnectionString(connectionString.ConnectionString))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ThingMap>());
		}

		public void Registered(ISessionFactory factory)
		{
		}
	}
}