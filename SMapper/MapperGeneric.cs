#nullable enable

namespace SMapper
{
    public static class Mapper<T> where T : new()
	{
		/// <summary>
		/// Создает экземпляр класса и инициилизирует его свойства значениями 
		/// из одноименных свойств класса переданного в аргументе.
		/// Используются параметры определения одноименных полей по умолчанию
		/// </summary>
		/// <param name="origin">класс источник</param>        
		/// <exception cref="MapperException">ошибка при трансляции полей</exception>
		/// <returns>Экземпляр класса инициилизированный значенями из класса источника</returns>
		public static T Convert(object origin) => Convert(origin, MapperSettings.Default);

		/// <summary>
		/// Создает экземпляр класса и инициилизирует его свойства значениями 
		/// из одноименных свойств класса переданного в аргументе.
		/// </summary>
		/// <param name="origin">класс источник</param>
		/// <param name="settings">параметры определения одноименных полей</param>
		/// <param name='converters'>конвертеры</param>
		/// <exception cref="MapperException">ошибка при трансляции полей</exception>
		/// <returns>Экземпляр класса инициилизированный значенями из класса источника</returns>
		public static T Convert(object origin, MapperSettings settings, params BaseMapperConverter[] converters)
		{
			if (origin == null) throw new ArgumentNullException(nameof(origin));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			T translate = new T();
			var originProperties = MapperPropertyInfo.GetPrompterPropertyInfos(origin.GetType(), settings);
			var translateProperties = MapperPropertyInfo.GetPrompterPropertyInfos(translate.GetType(), settings);

			foreach (var tProp in translateProperties)
			{
				var oProp = originProperties.FirstOrDefault(x => x.Alias == tProp.Alias);
				if (oProp != null)
				{
					if (converters.GetConverter(oProp.PropertyType, tProp.PropertyType, out var converter))
					{
						converter.InnerConvert(origin, translate, oProp.PropertyInfo, tProp.PropertyInfo);
					}
					else if (tProp.PropertyType == oProp.PropertyType)
					{
						tProp.SetValue(translate, oProp.GetValue(origin));
					}
					else
					{
						throw new MapperTypeException($"detect different types. " +
													$"origin.{oProp.OriginName} is {oProp.PropertyType} but translate.{tProp.OriginName} is {tProp.PropertyType}");
					}
				}
			}

			return translate;
		}
	}
}
