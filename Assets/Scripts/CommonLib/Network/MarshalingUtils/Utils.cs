/*using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;

public class Utils
{
	private static void CreatePropertyAttribute(PropertyBuilder propertyBuilder, Type attributeType, Array parameterValues)
	{
		var parameterTypes = (from object t in parameterValues select t.GetType()).ToArray();
		var propertyAttributeInfo = typeof(RangeAttribute).GetConstructor(parameterTypes);

		if (propertyAttributeInfo != null)
		{
			var customAttributeBuilder = new CustomAttributeBuilder(propertyAttributeInfo,
					parameterValues.Cast<object>().ToArray());
			propertyBuilder.SetCustomAttribute(customAttributeBuilder);
		}
	}

	private static PropertyBuilder CreateAutomaticProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
	{
		var propertyName = propertyInfo.Name;
		var propertyType = propertyInfo.PropertyType;

		// Generate a private field
		var field = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

		// Generate a public property
		var property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType,
				null);

		// The property set and property get methods require a special set of attributes:
		const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

		// Define the "get" accessor method for current private field.
		var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, getSetAttr, propertyType, Type.EmptyTypes);

		// Intermediate Language stuff...
		var currGetIl = currGetPropMthdBldr.GetILGenerator();
		currGetIl.Emit(OpCodes.Ldarg_0);
		currGetIl.Emit(OpCodes.Ldfld, field);
		currGetIl.Emit(OpCodes.Ret);

		// Define the "set" accessor method for current private field.
		var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName, getSetAttr, null, new[] {propertyType});

		// Again some Intermediate Language stuff...
		var currSetIl = currSetPropMthdBldr.GetILGenerator();
		currSetIl.Emit(OpCodes.Ldarg_0);
		currSetIl.Emit(OpCodes.Ldarg_1);
		currSetIl.Emit(OpCodes.Stfld, field);
		currSetIl.Emit(OpCodes.Ret);

		// Last, we must map the two methods created above to our PropertyBuilder to 
		// their corresponding behaviors, "get" and "set" respectively. 
		property.SetGetMethod(currGetPropMthdBldr);
		property.SetSetMethod(currSetPropMthdBldr);

		return property;
	}

	public static object EditingObject(object obj)
	{
		// Create the typeBuilder
		var assembly = new AssemblyName("EditingWrapper");
		var appDomain = Thread.GetDomain();
		var assemblyBuilder = appDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
		var moduleBuilder = assemblyBuilder.DefineDynamicModule(assembly.Name);

		// Create the class
		var typeBuilder = moduleBuilder.DefineType("EditingWrapper",
				TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
				TypeAttributes.BeforeFieldInit, typeof(object));

		var objType = obj.GetType();

		foreach (var propertyInfo in objType.GetProperties())
		{
			var propertyName = propertyInfo.Name;
			var propertyType = propertyInfo.PropertyType;

			// Create an automatic property
			var propertyBuilder = CreateAutomaticProperty(typeBuilder, propertyInfo);

			// Set Range attribute
			CreatePropertyAttribute(propertyBuilder, typeof(Category), new[] {"My new category value"});
		}

		// Generate our type
		var generetedType = typeBuilder.CreateType();

		// Now we have our type. Let's create an instance from it:
		var generetedObject = Activator.CreateInstance(generetedType);

		return generetedObject;
	}
}*/