#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections;
using System.Transactions;
using Castle.Facilities.AutoTx;
using Castle.Facilities.NHibernate.Tests.Framework;
using Castle.Facilities.NHibernate.Tests.TestClasses;
using Castle.MicroKernel.Registration;
using Castle.Services.Transaction;
using Castle.Windsor;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NUnit.Framework;
using ITransaction = NHibernate.ITransaction;
using Castle.Facilities.AutoTx.Testing;

namespace Castle.Facilities.NHibernate.Tests
{
	internal class ValidationError_OnSave : EnsureSchema
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ValidationError_OnSave));

		private Container _Container;

		[SetUp]
		public void SetUp()
		{
			BasicConfigurator.Configure();
			_Container = new Container();
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void RunTest()
		{
			_Logger.Debug("starting test run");

			using (var x = _Container.ResolveScope<Test>())
				x.Service.Run();
		}
	}

	internal class Container : WindsorContainer
	{
		public Container()
		{
			Register(Component.For<INHibernateInstaller>().Instance(new ExampleInstaller(new ThrowingInterceptor())));
			
			AddFacility<AutoTxFacility>();
			AddFacility<NHibernateFacility>();

			Register(Component.For<Test>().LifeStyle.Transient);
		}
	}

	internal class ThrowingInterceptor : IInterceptor
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (ThrowingInterceptor));

		public bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState,
		                         string[] propertyNames, IType[] types)
		{
			_Logger.Debug("throwing validation exception");

			throw new ApplicationException("imaginary validation error");
		}

		#region unused

		public bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			return false;
		}

		public int[] FindDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames,
		                       IType[] types)
		{
			return null;
		}

		public object GetEntity(string entityName, object id)
		{
			return null;
		}

		public void AfterTransactionBegin(ITransaction tx)
		{
		}

		public void BeforeTransactionCompletion(ITransaction tx)
		{
		}

		public void AfterTransactionCompletion(ITransaction tx)
		{
		}

		public string GetEntityName(object entity)
		{
			return null;
		}

		public object Instantiate(string entityName, EntityMode entityMode, object id)
		{
			return null;
		}

		public bool? IsTransient(object entity)
		{
			throw new NotImplementedException();
		}

		public void OnCollectionRecreate(object collection, object key)
		{
			throw new NotImplementedException();
		}

		public void OnCollectionRemove(object collection, object key)
		{
			throw new NotImplementedException();
		}

		public void OnCollectionUpdate(object collection, object key)
		{
			throw new NotImplementedException();
		}

		public void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			throw new NotImplementedException();
		}

		public bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			return false;
		}

		public void PostFlush(ICollection entities)
		{
		}

		public void PreFlush(ICollection entities)
		{
		}

		public void SetSession(ISession session)
		{
		}

		#endregion

		public SqlString OnPrepareStatement(SqlString sql)
		{
			return sql;
		}
	}

	public class Test
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (Test));

		private readonly ISessionManager _SessionManager;
		private Guid _ThingId;

		public Test(ISessionManager sessionManager)
		{
			_SessionManager = sessionManager;
		}

		public virtual void Run()
		{
			_Logger.Debug("run invoked");

			SaveNewThing();
			try
			{
				_Logger.Debug("chaning thing which will throw");

				ChangeThing();
			}
			catch (TransactionAbortedException)
			{
				// this exception is expected - it is thrown by the validator
			}

			// loading a new thing, in a new session!!
			var t = LoadThing();
		}

		[Transaction]
		protected virtual void SaveNewThing()
		{
			var s = _SessionManager.OpenSession();
			var thing = new Thing(18.0);
			_ThingId = (Guid) s.Save(thing);
		}

		[Transaction]
		protected virtual void ChangeThing()
		{
			var s = _SessionManager.OpenSession();
			var thing = s.Load<Thing>(_ThingId);
			thing.Value = 19.0;
		}

		[Transaction]
		protected virtual Thing LoadThing()
		{
			var s = _SessionManager.OpenSession(); // we are expecting this to be a new session
			return s.Load<Thing>(_ThingId);
		}
	}
}