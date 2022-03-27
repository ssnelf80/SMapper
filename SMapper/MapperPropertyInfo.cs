#nullable enable
using System.Reflection;

namespace SMapper
{
	internal class MapperPropertyInfo
	{
		public bool Ignore { get; private set; }
		public string OriginName { get; private set; }
		public string Alias { get; private set; }
		public PropertyInfo PropertyInfo { get; private set; }

		public Type PropertyType => PropertyInfo.PropertyType;
		public object? GetValue(object? obj) => PropertyInfo.GetValue(obj);
		public void SetValue(object? obj, object? value) => PropertyInfo.SetValue(obj, value);

		public MapperPropertyInfo(PropertyInfo prop, MapperSettings settings)
		{
			PropertyInfo = prop;
			OriginName = prop.Name;
			SetAlias(prop.Name, settings);
			Modify(prop, settings);
		}

		public void SetAlias(string value, MapperSettings settings)
		{
			Alias = value;
			if (settings.IgnoreUndercase)
			{
				Alias = Alias.Replace("_", "");
			}

			if (settings.ToLowerCase)
			{
				Alias = Alias.ToLower();
			}
		}

		public static IEnumerable<MapperPropertyInfo> GetPrompterPropertyInfos(Type type, MapperSettings settings)
		{
			var propMap = new Dictionary<string, MapperPropertyInfo>();

			var considered = new List<Type>();
			var queue = new Queue<Type>();
			considered.Add(type);
			queue.Enqueue(type);

			while (queue.Count > 0)
			{
				var subType = queue.Dequeue();
				foreach (var subInterface in subType.GetInterfaces())
				{
					if (considered.Contains(subInterface)) continue;

					considered.Add(subInterface);
					queue.Enqueue(subInterface);
				}

				var typeProperties = subType.GetProperties(
					BindingFlags.FlattenHierarchy
					| BindingFlags.Public
					| BindingFlags.Instance);

				foreach (var prop in typeProperties)
				{
					if (propMap.TryGetValue(prop.Name, out var val))
					{
						val.Modify(prop, settings);
					}
					else
					{
						propMap.Add(prop.Name, new MapperPropertyInfo(prop, settings));
					}
				}
			}

			var res = propMap.Values.Where(x => !x.Ignore);
			if (res.Select(x => x.Alias).Distinct().Count() != res.Count())
			{
				throw new MapperAmbiguousException(
					$"{type} has ambiguous interpretation of 'known as' aliases. try change Prompter settings");
			}
			else
			{
				return res;
			}
		}

		public MapperPropertyInfo Modify(PropertyInfo prop, MapperSettings settings)
		{
			if (prop.CustomAttributes.Any(x => x.AttributeType == typeof(MapperIgnore)))
			{
				this.Ignore = true;
			}
			else
			{
				var rule = prop.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(MapperAlias));
				if (rule != null)
				{
					var arg = rule.NamedArguments.FirstOrDefault(x => x.MemberName == "Alias");
					var name = arg.TypedValue.Value as string
						?? rule.ConstructorArguments.FirstOrDefault().Value as string;
					if (string.IsNullOrWhiteSpace(name))
						throw new MapperAliasException(
							$"alias for property {prop.Name} is null or whitespace");
					this.SetAlias(name, settings);
				}
			}

			return this;
		}
	}
}
