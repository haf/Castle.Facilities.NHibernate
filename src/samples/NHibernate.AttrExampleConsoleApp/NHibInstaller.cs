using System;
using Castle.Facilities.NHibernate;
using Castle.Transactions;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.Attributes;

namespace NHibernate.AttrExampleConsoleApp
{
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

        public Func<Configuration> ConfigBuilder
        {
            get
            {
                HbmSerializer.Default.Validate = true;
                Configuration cfg = new Configuration()
                    .DataBaseIntegration(db =>
                                             {
                                                 db.ConnectionString = "Data Source=DataStore.db;Version=3";
                                                 db.Dialect<SQLiteDialect>();
                                                 db.Driver<SQLite20Driver>();
                                                 db.ConnectionProvider<DriverConnectionProvider>();
                                             })
                    .AddAssembly(GetType().Assembly)
                    .AddInputStream(HbmSerializer.Default.Serialize(GetType().Assembly));
                return () => cfg;
            }
        }

        public void Registered(ISessionFactory factory)
        {
        }
    }
}