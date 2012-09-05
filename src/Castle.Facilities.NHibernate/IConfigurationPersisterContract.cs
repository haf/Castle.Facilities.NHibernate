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

using NHibernate.Cfg;

namespace Castle.Facilities.NHibernate
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Collections.Generic;

	[ContractClassFor( typeof( IConfigurationPersister ) )]
	internal abstract class IConfigurationPersisterContract : IConfigurationPersister
	{
		public Configuration ReadConfiguration(string filename)
		{
			Contract.Requires( filename != null );

			throw new NotImplementedException();
		}

		public void WriteConfiguration(string filename, Configuration cfg)
		{
			Contract.Requires( filename != null );
			Contract.Requires( cfg != null );

			throw new NotImplementedException();
		}

		public bool IsNewConfigurationRequired(string filename, IList<string> dependencies)
		{
			Contract.Requires( filename != null );
			Contract.Requires( dependencies != null );

			throw new NotImplementedException();
		}
	}
}