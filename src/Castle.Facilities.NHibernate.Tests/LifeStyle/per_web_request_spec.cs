using System;
using Castle.Facilities.AutoTx;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NHibernate;
using NUnit.Framework;
using Castle.Facilities.AutoTx.Testing;

namespace Castle.Facilities.NHibernate.Tests.LifeStyle
{
	public class per_web_request_spec
	{
		private IWindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer()
			.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>())
			.AddFacility<AutoTxFacility>()
			.AddFacility<NHibernateFacility>(
				fac => fac.DefaultLifeStyle = DefaultSessionLifeStyleOption.SessionPerWebRequest);
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void RegisterAndResolve()
		{
			try
			{
				using (var scope = _Container.ResolveScope<ISession>())
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
			Assert.Throws<MissingTransactionException>(() => _Container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionPerTxSuffix));
		}

		[Test]
		public void resolving_transient()
		{
			var s1 = _Container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionTransientSuffix);
			var s2 = _Container.Resolve<ISession>(ExampleInstaller.Key + NHibernateFacility.SessionTransientSuffix);
			Assert.That(s1.GetSessionImplementation().SessionId, Is.Not.EqualTo(s2.GetSessionImplementation().SessionId));
			_Container.Release(s1);
			_Container.Release(s2);
		}
	}
}