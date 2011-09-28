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

	using Castle.MicroKernel.Facilities;

	using global::NHibernate;

	/// <summary>
	/// 	The session manager is an object wrapper around the "real" manager which is managed
	/// 	by a custom per-transaction lifestyle. If you wish to implement your own manager, you can
	/// 	pass a function to this object at construction time and replace the built-in session manager.
	/// </summary>
	public class SessionManager : ISessionManager
	{
		private readonly Func<ISession> getSession;

		/// <summary>
		/// 	Constructor.
		/// </summary>
		/// <param name = "getSession"></param>
		public SessionManager(Func<ISession> getSession)
		{
			Contract.Requires(getSession != null);
			Contract.Ensures(this.getSession != null);

			this.getSession = getSession;
		}

		ISession ISessionManager.OpenSession()
		{
			var session = getSession();

			if (session == null)
				throw new FacilityException(
					"The Func<ISession> passed to SessionManager returned a null session. Verify your registration.");

			return session;
		}
	}
}