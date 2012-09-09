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

	using Transactions;

	using global::NHibernate.Cfg;
	using global::NHibernate;

	/// <summary>
	/// 	Register a bunch of these; one for each database.
	/// </summary>
	[ContractClass(typeof(INHibernateInstallerContract))]
	public interface INHibernateInstaller
	{
		/// <summary>
		/// 	Is this the default session factory
		/// </summary>
		bool IsDefault { get; }

		/// <summary>
		/// 	Gets a session factory key. This key must be unique for the registered
		/// 	NHibernate installers.
		/// </summary>
		string SessionFactoryKey { get; }

		/// <summary>
		/// 	An interceptor to assign to the ISession being resolved through this session factory.
		/// </summary>
		Maybe<IInterceptor> Interceptor { get; }

		/// <summary>
		/// Returns NHibernate configuration
		/// </summary>
		Configuration Config { get; }

		/// <summary>
		/// 	Call-back to the installer, when the factory is registered
		/// 	and correctly set up in Windsor..
		/// </summary>
		/// <param name = "factory"></param>
		void Registered(ISessionFactory factory);
	}
}