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
	using System.Collections;
	using System.Transactions;

	using Castle.Facilities.AutoTx;
	using Castle.Facilities.AutoTx.Testing;
	using Castle.Facilities.NHibernate.Tests.Framework;
	using Castle.Facilities.NHibernate.Tests.TestClasses;
	using Castle.MicroKernel.Registration;
	using Castle.Services.Transaction;
	using Castle.Windsor;

	using NLog;

	using NUnit.Framework;

	using global::NHibernate;
	using global::NHibernate.SqlCommand;
	using global::NHibernate.Type;

	using ITransaction = global::NHibernate.ITransaction;

	internal class ValidationError_OnSave : EnsureSchema
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private Container container;

		[SetUp]
		public void SetUp()
		{
			container = new Container();
		}

		[TearDown]
		public void TearDown()
		{
			container.Dispose();
		}

		[Test]
		public void RunTest()
		{
			logger.Debug("starting test run");

			using (var x = container.ResolveScope<Test>())
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
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState,
		                         string[] propertyNames, IType[] types)
		{
			logger.Debug("throwing validation exception");

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
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly ISessionManager sessionManager;
		private Guid thingId;

		public Test(ISessionManager sessionManager)
		{
			this.sessionManager = sessionManager;
		}

		public virtual void Run()
		{
			logger.Debug("run invoked");

			SaveNewThing();
			try
			{
				logger.Debug("chaning thing which will throw");

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
			var s = sessionManager.OpenSession();
			var thing = new Thing(18.0);
			thingId = (Guid)s.Save(thing);
		}

		[Transaction]
		protected virtual void ChangeThing()
		{
			var s = sessionManager.OpenSession();
			var thing = s.Load<Thing>(thingId);
			thing.Value = 19.0;
		}

		[Transaction]
		protected virtual Thing LoadThing()
		{
			var s = sessionManager.OpenSession(); // we are expecting this to be a new session
			return s.Load<Thing>(thingId);
		}
	}
}