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

namespace Castle.Facilities.NHibernate.Tests
{
	using Castle.Facilities.AutoTx;
	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.NHibernate.Tests.Framework;
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Transactions;
	using Castle.Windsor;

	using NLog;

	using NUnit.Framework;

	public class SimpleUseCase_ProtectedMethod : EnsureSchema
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private WindsorContainer c;

		[SetUp]
		public void SetUp()
		{
			c = GetWindsorContainer();
		}

		[TearDown]
		public void TearDown()
		{
			logger.Debug("running tear-down, removing components");

			c.Register(Component.For<TearDownService>().LifeStyle.Transient);

			using (var s = c.ResolveScope<TearDownService>())
				s.Service.ClearThings();

			c.Dispose();
		}

		private static WindsorContainer GetWindsorContainer()
		{
			var c = new WindsorContainer();

			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());

			c.AddFacility<AutoTxFacility>();
			c.AddFacility<NHibernateFacility>();

			Assert.That(c.Kernel.HasComponent(typeof(ITransactionManager)));

			return c;
		}

		[Test]
		public void Register_Run()
		{
			c.Register(Component.For<ServiceWithProtectedMethodInTransaction>());

			using (var s = c.ResolveScope<ServiceWithProtectedMethodInTransaction>())
				s.Service.Do();
		}
	}
}