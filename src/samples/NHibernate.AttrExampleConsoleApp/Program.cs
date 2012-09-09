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

namespace NHibernate.AttrExampleConsoleApp
{
    using System;

    using Castle.Facilities.AutoTx;
    using Castle.Facilities.AutoTx.Testing;
    using Castle.Facilities.NHibernate;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

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
                cfg.UseNLog();
                cfg.RunAsNetworkService();

                cfg.SetDescription("A service logging when it starts and stops to SQLite.");
                cfg.SetDisplayName("NHibernate Logger");
                cfg.SetServiceName("nhibLogger");

                cfg.Service<Program>(s =>
                {
                    s.ConstructUsing(() => new Program());
                    s.WhenStarted(p => p.Start());
                    s.WhenStopped(p => p.Stop());
                });
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
                {
                    new SchemaUpdate(up.Service).Execute(false, true);
                }

                Console.WriteLine("Current log contents:");
                Console.WriteLine("[utc date] - [text]");
                Console.WriteLine("-------------------");
                scope.Service.ReadLog(Console.WriteLine); // read everything from saved log
                scope.Service.WriteToLog(string.Format("{0} - Started", DateTime.UtcNow)); // write simple line to log
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
}
