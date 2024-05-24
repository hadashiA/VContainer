using System;

namespace Reflex
{
    internal readonly struct TypeInfo
    {
        public readonly Type[] ConstructorParameters;
        public readonly DynamicObjectActivator Activator;

        public TypeInfo(Type[] constructorParameters, DynamicObjectActivator activator)
        {
            Activator = activator;
            ConstructorParameters = constructorParameters;
        }
    }
}