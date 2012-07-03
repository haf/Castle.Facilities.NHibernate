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

using System.Web;
using Castle.MicroKernel.Lifestyle;

namespace Castle.Facilities.NHibernate.Tests.LifeStyle
{
	using System;

	using Castle.Facilities.AutoTx;
	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	using global::NHibernate;

	public class per_web_request_spec
	{
		private IWindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer()
				.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>())
				.AddFacility<AutoTxFacility>()
				.AddFacility<NHibernateFacility>(
					fac => fac.DefaultLifeStyle = DefaultSessionLifeStyleOption.SessionPerWebRequest);
			var app = new HttpApplication();
			var lifestyle = new PerWebRequestLifestyleModule();
			lifestyle.Init(app);
		}

		[TearDown]
		public void TearDown()
		{
			container.Dispose();
		}

		[Test]
		public void RegisterAndResolve()
		{
			try
			{
				using (var scope = container.ResolveScope<ISession>())
					Console.WriteLine(scope.Service.GetSessionImplementation().SessionId);

				Assert.Fail("Not in web request, should not resolve.");
			}
			catch (InvalidOperationException e)
			{
				Assert.That(e.Message, Is.StringContaining("HttpContext.Current"));
			}
		}

		[Test]
		public void resolving_per_tx()
		{
			Assert.Throws<MissingTransactionException>(() => container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPerTxSuffix));
		}

		[Test]
		public void resolving_transient()
		{
			var s1 = container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionTransientSuffix);
			var s2 = container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionTransientSuffix);
			Assert.That(s1.GetSessionImplementation().SessionId, Is.Not.EqualTo(s2.GetSessionImplementation().SessionId));
			container.Release(s1);
			container.Release(s2);
		}
	}
}