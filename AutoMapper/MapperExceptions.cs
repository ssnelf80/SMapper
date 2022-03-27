namespace SMapper
{
    public class MapperException : Exception
	{
		public MapperException(string message) : base(message)
		{
		}
	}

	public class MapperAliasException : MapperException
	{
		public MapperAliasException(string message) : base(message)
		{
		}
	}

	public class MapperTypeException : MapperException
	{
		public MapperTypeException(string message) : base(message)
		{
		}
	}

	public class MapperAmbiguousException : MapperException
	{
		public MapperAmbiguousException(string message) : base(message)
		{
		}
	}


}
