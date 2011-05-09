using System;
using Castle.Facilities.AutoTx;
using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Services.Transaction;
using Castle.Windsor;
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests
{
	public class AdvancedUseCase_DependentTransactionsAreNotFlushingToStore
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof(SimpleUseCase_SingleSave));

		private WindsorContainer c;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
			c = GetWindsorContainer();
		}

		[Test, Ignore("Haven't found a repro yet")]
		public void MainTx_ThenRW_ThenR_ThenDependentWrite()
		{
			// given
			var component = c.Resolve<ReproClass>();
			component.SaveNewThingSetup();

			// then
			component.MainInvocation();
			Assert.Fail("we haven't solved this test yet");
		}


		private static WindsorContainer GetWindsorContainer()
		{
			var c = new WindsorContainer();

			c.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());

			c.AddFacility<AutoTxFacility>();
			c.AddFacility<NHibernateFacility>();

			c.Register(Component.For<ReproClass>());

			Assert.That(c.Kernel.HasComponent(typeof(ITransactionManager)));

			return c;
		}
	}

	public class ReproClass
	{
		private readonly Func<ISession> _GetSession;
		private readonly ITransactionManager _Manager;
		private Guid _ThingId;

		public ReproClass(Func<ISession> getSession, ITransactionManager manager)
		{
			if (getSession == null) throw new ArgumentNullException("getSession");
			_GetSession = getSession;
			_Manager = manager;
		}

		[Transaction]
		public virtual Guid SaveNewThingSetup()
		{
			return _ThingId = (Guid)_GetSession().Save(new Thing(19.0));
		}

		[Transaction]
		public virtual void MainInvocation()
		{
			var t = Read1();
			Write1(t);

			Read2();
			Write2InTx(t);

		}

		[Transaction]
		protected virtual void Write2InTx(Thing t)
		{
			Assert.That(_Manager.Count, Is.EqualTo(2));
			_GetSession().Delete(t);
		}

		private void Read2()
		{
			_GetSession().Load<Thing>(_ThingId);
		}

		private void Write1(Thing thing)
		{
			thing.Value = 20.0;
			_GetSession().Update(thing);
		}

		private Thing Read1()
		{
			return _GetSession().Load<Thing>(_ThingId);
		}
	}
}