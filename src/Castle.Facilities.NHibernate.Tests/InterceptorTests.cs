using Castle.Facilities.AutoTx;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NHibernate;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests
{
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
			{
				Assert.That(session.GetSessionImplementation().Interceptor, Is.Not.Null);
			}
			w.Release(session);
		}
	}
}