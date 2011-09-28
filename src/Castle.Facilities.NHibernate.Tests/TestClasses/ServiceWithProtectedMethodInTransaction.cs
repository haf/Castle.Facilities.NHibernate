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

	using Castle.Services.Transaction;

	using NUnit.Framework;

	using global::NHibernate;

	public class ServiceWithProtectedMethodInTransaction
	{
		private readonly ISessionFactory factory;

		public ServiceWithProtectedMethodInTransaction(ISessionFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");
			this.factory = factory;
		}

		public void Do()
		{
			var id = SaveIt();
			ReadAgain(id);
		}

		protected void ReadAgain(Guid id)
		{
			using (var s = factory.OpenSession())
			{
				var t = s.Load<Thing>(id);
				Assert.That(t.Id, Is.EqualTo(id));
			}
		}

		[Transaction]
		protected virtual Guid SaveIt()
		{
			using (var s = factory.OpenSession())
				return (Guid)s.Save(new Thing(45.0));
		}
	}
}