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
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	using global::NHibernate;

	public class InterceptorTests
	{
		[Test]
		public void NonNullInterceptor()
		{
			var w = new WindsorContainer()
				.Register(Component.For<INHibernateInstaller>().Instance(new ExampleInstaller(new ThrowingInterceptor())))
				.AddFacility<AutoTxFacility>()
				.AddFacility<NHibernateFacility>();

			var session = w.Resolve<ISession>(w.Resolve<INHibernateInstaller>().SessionFactoryKey + NHibernateFacility.SessionTransientSuffix);
			using (session)
				Assert.That(session.GetSessionImplementation().Interceptor, Is.Not.Null);
			w.Release(session);
		}
	}
}