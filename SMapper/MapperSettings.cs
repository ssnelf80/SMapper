#nullable enable

namespace SMapper
{
	public class MapperSettings
	{
		public bool ToLowerCase { get; set; }
		public bool IgnoreUndercase { get; set; }

		public static MapperSettings Default => new()
		{
			ToLowerCase = true,
			IgnoreUndercase = true
		};

		public static MapperSettings Ignore => new()
		{
			ToLowerCase = false,
			IgnoreUndercase = false
		};
	}
}
