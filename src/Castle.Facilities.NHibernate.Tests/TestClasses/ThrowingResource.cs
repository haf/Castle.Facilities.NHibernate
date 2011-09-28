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
	using System.Transactions;

	internal class ThrowingResource : ISinglePhaseNotification
	{
		private readonly bool throwIt;
		private int errorCount;

		public ThrowingResource(bool throwIt)
		{
			this.throwIt = throwIt;
		}

		public bool WasRolledBack { get; private set; }

		#region Implementation of IEnlistmentNotification

		void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		void IEnlistmentNotification.Commit(Enlistment enlistment)
		{
			if (throwIt && ++errorCount < 2)
				throw new ApplicationException("simulating resource failure");

			enlistment.Done();
		}

		void IEnlistmentNotification.Rollback(Enlistment enlistment)
		{
			WasRolledBack = true;

			enlistment.Done();
		}

		void IEnlistmentNotification.InDoubt(Enlistment enlistment)
		{
			enlistment.Done();
		}

		void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
		{
		}

		#endregion
	}
}