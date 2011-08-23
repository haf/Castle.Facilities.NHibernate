#region license
// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
#endregion

using System;
using Castle.Facilities.AutoTx;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NHibernate;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests.LifeStyle
{
	public class transient_resolving_spec
	{
		private IWindsorContainer _Container;

		[SetUp]
		public void given_transient_registration()
		{
			_Container = new WindsorContainer()
				.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>())
				.AddFacility<AutoTxFacility>()
				.AddFacility<NHibernateFacility>(
				fac => fac.DefaultLifeStyle = DefaultSessionLifeStyleOption.SessionTransient);
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void then_all_new_ids()
		{
			var s = _Container.Resolve<Func<ISession>>();
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
				() => _Container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPerTxSuffix));
		}

		[Test]
		public void then_per_web_throws()
		{
			Assert.Throws<InvalidOperationException>(
				() => _Container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPWRSuffix));
		}
	}
}