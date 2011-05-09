using System;
using Castle.Facilities.AutoTx;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NHibernate;
using NUnit.Framework;
using Castle.Facilities.AutoTx.Testing;

namespace Castle.Facilities.NHibernate.Tests
{
	public class NotInWebRequestTests
	{
		[Test]
		public void RegisterAndResolve()
		{
			var w = new WindsorContainer()
				.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>())
				.AddFacility<AutoTxFacility>()
				.AddFacility<NHibernateFacility>(
				fac => fac.Option = DefaultSessionLifeStyleOption.SessionPerWebRequest);

			try
			{
				using (var scope = w.ResolveScope<ISession>())
					Console.WriteLine(scope.Service.GetSessionImplementation().SessionId);

				Assert.Fail("Not in web request, should not resolve.");
			}
			catch (InvalidOperationException e)
			{
				Assert.That(e.Message, Is.StringContaining("HttpContext.Current"));
			}
		}
	}
}