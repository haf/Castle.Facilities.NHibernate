using System;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	public class Thing
	{
		[Obsolete("NHibernate's c'tor")]
		protected Thing()
		{
		}

		public Thing(double val)
		{
			Value = val;
		}

		public Guid ID { get; protected set; }
		public double Value { get; set; }
	}
}