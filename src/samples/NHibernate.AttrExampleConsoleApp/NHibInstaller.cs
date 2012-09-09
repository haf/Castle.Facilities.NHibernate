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

        public Configuration Config
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
                return cfg;
            }
        }

        public void Registered(ISessionFactory factory)
        {
        }
    }
}