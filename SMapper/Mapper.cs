#nullable enable
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;


namespace SMapper
{
	public static class Mapper
	{
		private static AssemblyBuilder AssemblyBuilder = null;
		private static ModuleBuilder ModuleBuilder = null;
		private const MethodAttributes DEFAULT_GET_SET_ATTRIBUTES =
				   MethodAttributes.Public | MethodAttributes.SpecialName |
				   MethodAttributes.HideBySig;
		private static ModuleBuilder GetModuleBuilder()
		{
			if (AssemblyBuilder == null)
			{
				AssemblyName assemName = new AssemblyName();
				assemName.Name = "DynamicPrompterAssembly";
				AssemblyBuilder =
					AssemblyBuilder.DefineDynamicAssembly(assemName, AssemblyBuilderAccess.Run);

				ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DynamicPrompterModule");
			}
			return ModuleBuilder;
		}
		private static void CreateGetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
			FieldBuilder fieldBuilder, MapperPropertyInfo propInfo)
		{
			var methodGetBuilder =
				typeBuilder.DefineMethod("get_" + propInfo.Alias,
					DEFAULT_GET_SET_ATTRIBUTES,
					propInfo.PropertyType,
					Type.EmptyTypes);

			ILGenerator ilGenerator = methodGetBuilder.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
			ilGenerator.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(methodGetBuilder);
		}
		private static void CreateSetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
		  FieldBuilder fieldBuilder, MapperPropertyInfo propInfo)
		{
			var methodSetBuilder =
				typeBuilder.DefineMethod("set_" + propInfo.Alias,
					DEFAULT_GET_SET_ATTRIBUTES,
					null,
					new Type[] { propInfo.PropertyType });

			ILGenerator ilGenerator = methodSetBuilder.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
			ilGenerator.Emit(OpCodes.Ret);

			propertyBuilder.SetSetMethod(methodSetBuilder);
		}
		private static string GenerateTypeName(IEnumerable<MapperPropertyInfo> props)
		{
			StringBuilder sb = new();
			foreach (var prop in props)
			{
				sb.Append(prop.Alias).Append(prop.PropertyType);
			}

			using var md5gen = MD5.Create();
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
			var hash = md5gen.ComputeHash(stream);
			return $"Mapper_{BitConverter.ToString(hash)}";
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private static Type GetOrCreateType(IEnumerable<MapperPropertyInfo> propInfos)
		{
			var typeName = GenerateTypeName(propInfos);
			return GetModuleBuilder().GetType(typeName) ?? CreateType(propInfos, typeName);
		}
		private static Type CreateType(IEnumerable<MapperPropertyInfo> propInfos, string typeName)
		{
			TypeBuilder typeBuilder = GetModuleBuilder().DefineType(typeName, TypeAttributes.Public);
			foreach (var propInfo in propInfos)
			{
				var propertyBuilder = typeBuilder.DefineProperty(propInfo.Alias, PropertyAttributes.HasDefault,
					propInfo.PropertyType,
					Type.EmptyTypes);
				var fieldBuilder = typeBuilder.DefineField("_" + propInfo.Alias, propInfo.PropertyType,
					FieldAttributes.Private);

				CreateGetMethod(typeBuilder, propertyBuilder, fieldBuilder, propInfo);
				CreateSetMethod(typeBuilder, propertyBuilder, fieldBuilder, propInfo);
			}

			return typeBuilder.CreateType();
		}

		/// <summary>
		/// Создает экземпляр класса с динамически созданным типом и инициилизирует его свойства значениями 
		/// из одноименных свойств класса переданного в аргументе.
		/// </summary>
		/// <param name="origin">класс источник</param>        
		/// <exception cref="MapperException">ошибка при трансляции полей</exception>
		/// <returns>Экземляр класса динамически созданного типа инициилизированный значенями из класса источника</returns>
		public static object Convert(object origin)
		{
			if (origin == null) throw new ArgumentNullException(nameof(origin));
			var prompterBase = typeof(Mapper<>);
			var type = GetOrCreateType(MapperPropertyInfo.GetPrompterPropertyInfos(origin.GetType(), MapperSettings.Ignore));
			Type genericPrompterType = prompterBase.MakeGenericType(type);
			MethodInfo convertMethod = genericPrompterType.GetMethod("Convert", new Type[]
			{
				typeof(object), typeof(MapperSettings), typeof(BaseMapperConverter[])
			});
			return convertMethod.Invoke(null, new object[]
			{
				origin, MapperSettings.Ignore, null
			});
		}
	}
}
