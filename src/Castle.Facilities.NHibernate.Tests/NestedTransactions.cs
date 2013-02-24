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
	using Castle.Transactions;
	using Castle.Windsor;

	using NLog;

	using NUnit.Framework;

	using global::NHibernate;
	using global::NHibernate.SqlCommand;
	using global::NHibernate.Type;

	using ITransaction = global::NHibernate.ITransaction;

	internal class NestedTransactions : EnsureSchema
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

		/// <summary>
		/// This test shows that several transaction methods can be attached to the same transaction
		/// </summary>
		[Test]
		public void NestedTransactionFeature()
		{
			logger.Debug("starting test run");

			using (var x = container.ResolveScope<NestedTransactionService>())
				x.Service.Run();
		}

		/// <summary>
		/// This test shows that an objects created in a call chain is not persisted
		/// </summary>
		[Test]
		public void Transaction()
		{
			//Arrange
			logger.Debug("starting test run");
			var x = container.ResolveScope<NestedTransactionService>();
			int thingCount = x.Service.GetThingsCount();

			//Act
			x.Service.RunAndAssert();

			//Assert
			Assert.AreEqual(x.Service.GetThingsCount(), thingCount + 1, "A thing was created");
		}

		/// <summary>
		/// This test shows that objects are not persisted if something fails
		/// </summary>
		[Test]
		public void Rollback()
		{
			//Arrange
			logger.Debug("starting test run");
			var x = container.ResolveScope<NestedTransactionService>();
			int thingCount = x.Service.GetThingsCount();

			//Act
			try{
				x.Service.RunAndFail();
			}
			catch{
			}

			//Assert
			Assert.AreEqual(x.Service.GetThingsCount(), thingCount, "No thing was created");
		}
	}

	public class NestedTransactionService
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly ISessionManager sessionManager;
		private Guid thingId;

		public NestedTransactionService(ISessionManager sessionManager)
		{
			this.sessionManager = sessionManager;
		}

		[Transaction]
		public virtual void Run()
		{
			
			SaveNewThing();
			SaveNewThing();
		}

		[Transaction]
		public virtual void RunAndAssert()
		{
			//Arrange
			int thingCount = GetThingsCount();

			//Act
			SaveNewThing();

			//Assert
			Assert.AreEqual(GetThingsCount(), thingCount, "No thing was created yet");
		}

		[Transaction]
		public virtual void RunAndFail()
		{
			SaveNewThing();

			//Force a Fail so I can test the nested rollback
			throw new Exception();

			
		}
		

		[Transaction]
		protected virtual void SaveNewThing()
		{
			var s = sessionManager.OpenSession();
			var thing = new Thing(18.0);
			thingId = (Guid)s.Save(thing);
		}

		public virtual int GetThingsCount()
		{
			var s = sessionManager.OpenSession();
			return s.QueryOver<Thing>().RowCount();
		}
	}
}