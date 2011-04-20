using FluentNHibernate.Mapping;

namespace Castle.Facilities.NHibernate.Tests.TestClasses
{
	public class ThingMap : ClassMap<Thing>
	{
		public ThingMap()
		{
			Not.LazyLoad();
			Id(x => x.ID).GeneratedBy.GuidComb();
			Map(x => x.Value).Column("val");
		}
	}
}