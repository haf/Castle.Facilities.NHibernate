using System;
using System.Diagnostics.Contracts;
using Castle.Services.Transaction;
using NHibernate;
using log4net;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	public class TearDownService
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TearDownService));

		private readonly Func<ISession> _Session;

		public TearDownService(Func<ISession> session)
		{
			Contract.Requires(session != null);
			_Session = session;
		}

		[Transaction]
		public virtual void ClearThings()
		{
			_Logger.Debug("clearing things");
			_Session().Delete("from Thing");
		}
	}
}