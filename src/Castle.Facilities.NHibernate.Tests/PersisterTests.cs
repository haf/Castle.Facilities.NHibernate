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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

using NHibernate.Cfg;

using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests
{
	public class PersisterTests
	{
		private const string TestCache = "Test.Cache";

		[SetUp]
		[TearDown]
		public void SetUpTearDown()
		{
			if (File.Exists(TestCache))
				File.Delete(TestCache);
		}

		[Test]
		public void Can_Write_Config()
		{
			var sut = new FileConfigurationPersister();

			var nhibernateConfig = new Configuration();
			nhibernateConfig.SetProperty("blah", "value");

			sut.WriteConfiguration(TestCache, nhibernateConfig);
			
			Assert.That(File.Exists(TestCache), "Cache file doesn't exist");
		}

		[Test]
		public void Can_Write_And_Read_Config()
		{
			const string key = "key";
			const string value = "value";
			var sut = new FileConfigurationPersister();

			var nhibernateConfig = new Configuration();

			nhibernateConfig.SetProperty(key, value);

			sut.WriteConfiguration(TestCache, nhibernateConfig);

			Configuration result = sut.ReadConfiguration(TestCache);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Properties[key] == value, "Deserialized config doesn't contain property value");
		}
	}
}