using System.Reflection;

namespace SMapper
{
    public abstract class MapperConverter<TOrigin, TTranslate> : BaseMapperConverter
    {
		public MapperConverter() : this(typeof(TOrigin), typeof(TTranslate))
        {

        }

		private MapperConverter(Type from, Type to) : base(from, to)
        {
        }

		/// <summary>
		/// функция конвертации
		/// </summary>
		/// <param name="">оригинальный параметр</param>
		/// <returns>приведенный результат</returns>
		public abstract TTranslate Convert(TOrigin origin);     
		 

        public sealed override void InnerConvert(object origin, object translate, PropertyInfo originPropertyInfo, PropertyInfo translatePropertyInfo)
        {
			TOrigin originValue = (TOrigin)originPropertyInfo.GetValue(origin);
			translatePropertyInfo.SetValue(translate, Convert(originValue));
		}
    }

    public abstract class BaseMapperConverter
	{
		public Type From { get; private set; }
		public Type To { get; private set; }

		protected BaseMapperConverter(Type from, Type to)
		{
			From = from;
			To = to;
		}

		public abstract void InnerConvert(object origin, object translate, PropertyInfo originPropertyInfo,
			PropertyInfo translatePropertyInfo);
	}

	internal static class MapperConverterExtention
	{
		public static bool IsConverterFor(this BaseMapperConverter c, Type from, Type to) => from.Equals(c.From) && to.Equals(c.To);

		public static bool GetConverter(this BaseMapperConverter[] c, Type from, Type to,
			out BaseMapperConverter converter)
		{
			converter = c?.FirstOrDefault(x => x.IsConverterFor(from, to));
			return converter != null;
		}
	}
}
