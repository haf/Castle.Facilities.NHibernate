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
	using System.Diagnostics.Contracts;

	using global::NHibernate;

	/// <summary>
	/// 	Session manager interface. This denotes the ISession factory. The default
	/// 	session lifestyle is per-transaction, so call OpenSession within a transaction!
	/// </summary>
	[ContractClass(typeof(ISessionManagerContract))]
	public interface ISessionManager
	{
		/// <summary>
		/// 	Gets a new or existing ISession depending on your context.
		/// </summary>
		/// <returns>A non-null ISession.</returns>
		ISession OpenSession();
	}
}