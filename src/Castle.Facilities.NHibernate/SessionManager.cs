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

namespace Castle.Facilities.NHibernate
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Runtime.Remoting.Messaging;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.Transactions;
	using global::NHibernate;
	using ITransaction = Castle.Transactions.ITransaction;

	/// <summary>
	/// 	The session manager is an object wrapper around the "real" manager which is managed
	/// 	by a custom per-transaction lifestyle. If you wish to implement your own manager, you can
	/// 	pass a function to this object at construction time and replace the built-in session manager.
	/// </summary>
	public class SessionManager : ISessionManager
	{
		private readonly Func<ISession> getSession;
		private Guid privateSessionId = new Guid();
		private ITransactionManager transactionManager;

		/// <summary>
		/// 	Constructor.
		/// </summary>
		/// <param name = "getSession"></param>
		public SessionManager(Func<ISession> getSession, ITransactionManager transactionManager)
		{
			Contract.Requires(getSession != null);
			Contract.Ensures(this.getSession != null);

			this.getSession = getSession;
			this.transactionManager = transactionManager;
		}

		ISession ISessionManager.OpenSession()
		{
			Maybe<ITransaction> transaction = ObtainCurrentTransaction();

			//This is a new transaction or no transaction is required
			if (!transaction.HasValue)
			{
				var session = getSession();

				if (session == null)
					throw new FacilityException(
						"The Func<ISession> passed to SessionManager returned a null session. Verify your registration.");

				return session;
			}
			else
			{
				var session = GetStoredSession();

				//There is an active transaction but no session is created yet
				if (session == null)
				{
					session = getSession();

					if (session == null)
						throw new FacilityException(
							"The Func<ISession> passed to SessionManager returned a null session. Verify your registration.");

					//Store the session so I can reused
					StoreSession(session);

					//Attach to the TransactionEvent so I can clean the callcontext
					transaction.Value.Inner.TransactionCompleted += Inner_TransactionCompleted;
					
					return session;
				}
				else
				{
					return session;
				}
			}
		}

		/// <summary>
		/// Clear the CallContext when the transaction ends
		/// </summary>
		/// <param name="sender">Just event stuff</param>
		/// <param name="e">Just event stuff</param>
		void Inner_TransactionCompleted(object sender, System.Transactions.TransactionEventArgs e)
		{
			ClearStoredSession();
		}

		/// <summary>
		/// Gets the current transaction from de AutoTx facility via an ITransactionManager
		/// </summary>
		/// <returns>The current transaction</returns>
		private Maybe<ITransaction> ObtainCurrentTransaction()
		{
			return transactionManager.CurrentTransaction;
		}

		/// <summary>
		/// Stores a session in the CallContext
		/// </summary>
		/// <param name="session">current session</param>
		private void StoreSession(ISession session)
		{
			CallContext.SetData(privateSessionId.ToString(), session);
		}

		/// <summary>
		/// Retrieves the session stored in the callcontext
		/// </summary>
		/// <returns></returns>
		private ISession GetStoredSession()
		{
			return CallContext.GetData(privateSessionId.ToString()) as ISession;
		}

		/// <summary>
		/// Removes the session stored in the callcontext
		/// </summary>
		/// <returns></returns>
		private void ClearStoredSession()
		{
			CallContext.SetData(privateSessionId.ToString(), null);
		}

	}
}