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

namespace Castle.Facilities.NHibernate.Tests.LifeStyle
{
	using System;

	using Castle.Facilities.AutoTx;
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	using global::NHibernate;

	public class transient_resolving_spec
	{
		private IWindsorContainer container;

		[SetUp]
		public void given_transient_registration()
		{
			container = new WindsorContainer()
				.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>())
				.AddFacility<AutoTxFacility>()
				.AddFacility<NHibernateFacility>(
					fac => fac.DefaultLifeStyle = DefaultSessionLifeStyleOption.SessionTransient);
		}

		[TearDown]
		public void TearDown()
		{
			container.Dispose();
		}

		[Test]
		public void then_all_new_ids()
		{
			var s = container.Resolve<Func<ISession>>();
			var s1 = s();
			var s2 = s();

			Assert.That(
				s1.GetSessionImplementation().SessionId,
				Is.Not.EqualTo(s2.GetSessionImplementation().SessionId));
		}

		[Test]
		public void then_when_resolving_per_tx_throws_outside()
		{
			Assert.Throws<MissingTransactionException>(
				() => container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPerTxSuffix));
		}

		[Test]
		public void then_per_web_throws()
		{
			Assert.Throws<InvalidOperationException>(
				() => container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPWRSuffix));
		}
	}
}