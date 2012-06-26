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
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	using Castle.Facilities.AutoTx;
	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.NHibernate.Tests.Framework;
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Transactions;
	using Castle.Windsor;

	using NLog;

	using NUnit.Framework;

	using global::NHibernate;
	using Castle.Facilities.Logging;

	public class Multiple_Threads_DependentTransactionWithParent : EnsureSchema
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<LoggingFacility>(f => f.UseNLog());
			container.Register(Component.For<INHibernateInstaller>().ImplementedBy<ExampleInstaller>());
			container.AddFacility<AutoTxFacility>();
			container.AddFacility<NHibernateFacility>();
			container.Register(Component.For<ThreadedService>().LifeStyle.Transient);
		}

		[TearDown]
		public void TearDown()
		{
			container.Dispose();
		}

		[Test]
		public void SameSessionInSameTransaction()
		{
			using (var threaded = new ResolveScope<ThreadedService>(container))
				threaded.Service.VerifySameSession();
		}

		[Test]
		public void SameSession_WithRecursion()
		{
			using (var threaded = new ResolveScope<ThreadedService>(container))
				threaded.Service.VerifyRecursingSession();
		}

		[Test]
		public void Forking_NewTransaction_Means_AnotherISessionReference()
		{
			using (var threaded = new ResolveScope<ThreadedService>(container))
			{
				threaded.Service.MainThreadedEntry();
				Assert.That(threaded.Service.CalculationsIds.Count, Is.EqualTo(Environment.ProcessorCount));
			}
		}

		[Test]
		public void Forking_InDependentTransaction_Means_PerTransactionLifeStyle_SoSameInstances()
		{
			using (var threaded = new ResolveScope<ThreadedService>(container))
				threaded.Service.VerifySameSessionInFork();
		}
	}

	public class ThreadedService
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly List<Guid> calculationsIds = new List<Guid>();

		private readonly Func<ISession> getSession;
		private readonly ITransactionManager manager;
		private Guid mainThing;

		public ThreadedService(Func<ISession> getSession, ITransactionManager manager)
		{
			Contract.Requires(manager != null);
			Contract.Requires(getSession != null);

			this.getSession = getSession;
			this.manager = manager;
		}

		public List<Guid> CalculationsIds
		{
			get { return calculationsIds; }
		}

		#region Same instance tests

		[Transaction]
		public virtual void VerifySameSession()
		{
			var s = getSession();
			var id1 = s.GetSessionImplementation().SessionId;

			var s2 = getSession();
			Assert.That(s2.GetSessionImplementation().SessionId, Is.EqualTo(id1));
		}

		#endregion

		#region Recursion/multiple txs on call context

		[Transaction]
		public virtual void VerifyRecursingSession()
		{
			var myId = getSession().GetSessionImplementation().SessionId;
			CheckRecursingSession_ShouldBeSame(myId);
			CheckRecursingSessionWithoutTransaction_ShouldBeSame(myId);
			CheckForkedRecursingSession_ShouldBeDifferent(myId);
		}

		[Transaction]
		protected virtual void CheckRecursingSession_ShouldBeSame(Guid myId)
		{
			var session = getSession();
			Assert.That(myId, Is.EqualTo(session.GetSessionImplementation().SessionId));
		}

		[Transaction(Fork = true)]
		protected virtual void CheckForkedRecursingSession_ShouldBeDifferent(Guid myId)
		{
			var session = getSession();
			Assert.That(myId, Is.Not.EqualTo(session.GetSessionImplementation().SessionId));
		}

		protected virtual void CheckRecursingSessionWithoutTransaction_ShouldBeSame(Guid myId)
		{
			var session = getSession();
			Assert.That(myId, Is.EqualTo(session.GetSessionImplementation().SessionId));
		}

		#endregion

		#region Forking - Succeeding transactions

		[Transaction]
		public virtual void MainThreadedEntry()
		{
			var s = getSession();

			mainThing = (Guid)s.Save(new Thing(17.0));

			logger.Debug("put some cores ({0}) to work!", Environment.ProcessorCount);

			for (var i = 0; i < Environment.ProcessorCount; i++)
				CalculatePi(s.GetSessionImplementation().SessionId);
		}

		[Transaction(Fork = true)]
		protected virtual void CalculatePi(Guid firstSessionId)
		{
			var s = getSession();

			Assert.That(s.GetSessionImplementation().SessionId, Is.Not.EqualTo(firstSessionId),
			            "because ISession is not thread safe and we want per-transaction semantics when Fork=true");

			lock (calculationsIds)
				calculationsIds.Add((Guid)s.Save(new Thing(2*CalculatePiInner(1))));
		}

		protected double CalculatePiInner(int i)
		{
			if (i == 5000)
				return 1;

			return 1 + i/(2.0*i + 1)*CalculatePiInner(i + 1);
		}

		#endregion

		#region Forking: (PerTransaction = same sessions per tx)

		[Transaction]
		public virtual void VerifySameSessionInFork()
		{
			logger.Info("asserting for main thread");

			AssertSameSessionId();

			logger.Info("forking");
			VerifySameSessionInForkInner();
		}

		[Transaction(Fork = true)]
		protected virtual void VerifySameSessionInForkInner()
		{
			logger.Info("asserting for task-thread");
			AssertSameSessionId();
		}

		private void AssertSameSessionId()
		{
			var s1 = getSession().GetSessionImplementation().SessionId;
			var s2 = getSession().GetSessionImplementation().SessionId;

			if (!s1.Equals(s2))
				logger.Error("s1 != s2 in forked method");

			Assert.That(s1, Is.EqualTo(s2));
		}

		#endregion
	}
}