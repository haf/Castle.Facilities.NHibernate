using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NHibernate.Cfg;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	internal class PersistingInstaller : ExampleInstaller
	{
		public const string SerializedConfigFile = "NHibernateConfig.cache";
		private readonly IConfigurationPersister _persister;

		public PersistingInstaller(IConfigurationPersister persister)
		{
			_persister = persister;
		}

		public override Configuration Deserialize()
		{
			var dependencies = new[]
			{
				Assembly.GetExecutingAssembly().Location
			};

			if (!_persister.IsNewConfigurationRequired(SerializedConfigFile, dependencies.ToList()))
				return _persister.ReadConfiguration(SerializedConfigFile);

			return null;
		}

		public override void Serialize(Configuration configuration)
		{
			_persister.WriteConfiguration(SerializedConfigFile, configuration);
		}
	}
}