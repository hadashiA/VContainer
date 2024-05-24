using System.Reflection;

public readonly struct MonoInjectableInfo
{
	internal readonly FieldInfo[] InjectableFields;
	internal readonly PropertyInfo[] InjectableProperties;
	internal readonly MethodInfo[] InjectableMethods;

	public MonoInjectableInfo(FieldInfo[] injectableFields, PropertyInfo[] injectableProperties, MethodInfo[] injectableMethods)
	{
		InjectableFields = injectableFields;
		InjectableProperties = injectableProperties;
		InjectableMethods = injectableMethods;
	}
}