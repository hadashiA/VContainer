using System;

namespace Reflex
{
	internal static class TypeInfoRepository
	{
		internal static readonly Repository<Type, TypeInfo> Repository = new Repository<Type, TypeInfo>(BakeTypeInfo.Bake);
	}
}