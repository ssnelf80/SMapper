namespace SMapper
{
    [AttributeUsage(AttributeTargets.Property)]
	public class MapperAlias : Attribute
	{
		public virtual string Alias { get; set; }


		public MapperAlias()
		{

		}
		public MapperAlias(string alias)
		{
			Alias = alias;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class MapperIgnore : Attribute
	{
	}
}
