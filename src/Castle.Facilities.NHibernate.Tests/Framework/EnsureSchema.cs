using Castle.Facilities.NHibernate.Tests.TestClasses;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using NUnit.Framework;

namespace Castle.Facilities.NHibernate.Tests.Framework
{
	public abstract class EnsureSchema
	{
		[TestFixtureSetUp]
		public void Setup()
		{
			var configuration = new ExampleInstaller().BuildFluent().BuildConfiguration();
			new SchemaUpdate(configuration).Execute(true, true);
		}
	}
}