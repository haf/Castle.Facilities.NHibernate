using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using NHibernate;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{

	// this class uses the transient lifestyle because it resolves ISession in the constructor and
	// it's not resolving Func<ISession> or ISessionManager.
	public class ServiceUsingTransientSessionLifestyle
	{
		private readonly ISession _Session;
		private readonly IStatelessSession _StatelessSession;
		private Guid _ThingId;

		public ServiceUsingTransientSessionLifestyle(ISession session, IStatelessSession statelessSession)
		{
			Contract.Requires(session != null);
			_Session = session;
			_StatelessSession = statelessSession;
		}

		[Transaction]
		public virtual void SaveNewThing()
		{
			_ThingId = (Guid) _Session.Save(new Thing(4.6));
		}

		[Transaction]
		public virtual Thing VerifyThing()
		{
			Assert.That(_StatelessSession.Get<Thing>(_ThingId), Is.Not.Null);
			Assert.That(_StatelessSession.Transaction, Is.Not.Null);
			Assert.That(_Session.Transaction, Is.Not.Null);

			// for testing we need to make sure it's not just in the FLC.
			_Session.Clear();
			return _Session.Load<Thing>(_ThingId);
		}
	}
}