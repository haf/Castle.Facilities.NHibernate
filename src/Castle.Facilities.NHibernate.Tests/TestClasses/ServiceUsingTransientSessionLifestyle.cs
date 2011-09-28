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

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	using System;
	using System.Diagnostics.Contracts;

	using Castle.Services.Transaction;

	using NUnit.Framework;

	using global::NHibernate;

	// this class uses the transient lifestyle because it resolves ISession in the constructor and
	// it's not resolving Func<ISession> or ISessionManager.
	public class ServiceUsingTransientSessionLifestyle
	{
		private readonly ISession session;
		private readonly IStatelessSession statelessSession;
		private Guid thingId;

		public ServiceUsingTransientSessionLifestyle(ISession session, IStatelessSession statelessSession)
		{
			Contract.Requires(session != null);
			this.session = session;
			this.statelessSession = statelessSession;
		}

		[Transaction]
		public virtual void SaveNewThing()
		{
			thingId = (Guid)session.Save(new Thing(4.6));
		}

		[Transaction]
		public virtual Thing VerifyThing()
		{
			Assert.That(statelessSession.Get<Thing>(thingId), Is.Not.Null);
			Assert.That(statelessSession.Transaction, Is.Not.Null);
			Assert.That(session.Transaction, Is.Not.Null);

			// for testing we need to make sure it's not just in the FLC.
			session.Clear();
			return session.Load<Thing>(thingId);
		}
	}
}