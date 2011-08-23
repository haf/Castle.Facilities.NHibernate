using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using NHibernate;
using log4net;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	public class ServiceUsingPerTransactionSessionLifestyle
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ServiceUsingPerTransactionSessionLifestyle));

		private readonly Func<ISession> _GetSession;
		private Guid _Id;

		public ServiceUsingPerTransactionSessionLifestyle(Func<ISession> getSession)
		{
			Contract.Requires(getSession != null);
			_GetSession = getSession;
		}

		// a bit of documentation
		/// <remarks>
		/// 	<para>This method and the next demonstrate how you COULD use the factory delegate.
		/// 		EITHER you run dispose on the ISession, or you don't. In fact, NHibernate
		/// 		will dispose the ISession after the transaction is complete.</para>
		/// 	<para>This is what the log looks like with this code:</para>
		/// 	<para>
		/// 		2109 [TestRunnerThread] DEBUG Castle.Services.Transaction.Tests.vNext.ServiceUsingPerTransactionSessionLifestyle (null) - exiting using-block of session
		/// 		2109 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] running ISession.Dispose()
		/// 		System.Transactions Information: 0 : TransactionScope Created: <TraceSource>[Base]</TraceSource><TransactionTraceIdentifier><TransactionIdentifier>f5568393-d069-4e2d-b85c-5f928f4e64c7:1</TransactionIdentifier><CloneIdentifier>2</CloneIdentifier></TransactionTraceIdentifier><TransactionScopeResult>TransactionPassed</TransactionScopeResult>
		/// 		System.Transactions Information: 0 : Dependent Clone Created: <TraceSource>[Lightweight]</TraceSource><TransactionTraceIdentifier><TransactionIdentifier>f5568393-d069-4e2d-b85c-5f928f4e64c7:1</TransactionIdentifier><CloneIdentifier>3</CloneIdentifier></TransactionTraceIdentifier><DependentCloneOption>RollbackIfNotComplete</DependentCloneOption>
		/// 		2111 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - before transaction completion
		/// 	</para>
		/// 	<para>
		/// 		As you can see, there's no disposing of the ISession but until here:
		/// 	</para>
		/// 	<para>
		/// 		2163 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] executing real Dispose(True)
		/// 		2164 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - closing session
		/// 		2164 [TestRunnerThread] DEBUG NHibernate.AdoNet.AbstractBatcher (null) - running BatcherImpl.Dispose(true)
		/// 		2168 [TestRunnerThread] DEBUG Castle.Services.vNextTransaction.NHibernate.PerTransactionLifestyleManagerBase (null) - transaction#f5568393-d069-4e2d-b85c-5f928f4e64c7:1 completed, disposing object 'NHibernate.Impl.SessionImpl'
		/// 		2168 [TestRunnerThread] DEBUG NHibernate.Impl.SessionImpl (null) - [session-id=c2f5673f-f93f-43c9-ad27-c7df8f33edc7] running ISession.Dispose()
		/// 	</para>
		/// 	<para>
		/// 		It's impossible for the PerTransaction lifestyle to KNOW when the 'real' disposing of the ISession is, so it's still required to try and Release the component. However,
		/// 		this is not just to call Dispose on the ISession; it is also to let Windsor stop tracking the reference, which would otherwise lead to a memory leak.
		/// 	</para>
		/// </remarks>
		[Transaction]
		public virtual void SaveNewThing()
		{
			_Logger.DebugFormat("save new thing");

			using (var session = _GetSession())
			{
				// at KTH this is an arbitrary number
				_Id = (Guid) session.Save(new Thing(17.0));

				_Logger.DebugFormat("exiting using-block of session");
			}
		}

		[Transaction]
		public virtual Thing LoadNewThing()
		{
			// be aware how I'm not manually disposing the ISession here; I could, but it would make no difference
			return _GetSession().Get<Thing>(_Id);
		}
	}
}