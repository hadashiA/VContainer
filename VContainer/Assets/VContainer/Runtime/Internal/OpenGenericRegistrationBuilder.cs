using System;
using System.Collections.Generic;
using VContainer.Runtime.Internal.InstanceProviders;

namespace VContainer.Runtime.Internal
{
    public class OpenGenericRegistrationBuilder : RegistrationBuilder
    {
        public OpenGenericRegistrationBuilder(Type type, Type implementationType, Lifetime lifetime)
            : this(implementationType, lifetime)
        {
            As(type);
        }

        public OpenGenericRegistrationBuilder(Type implementationType, Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            if (!implementationType.IsGenericType || implementationType.IsConstructedGenericType)
                throw new VContainerException(implementationType, "Type is not open generic type.");
        }

        public override Registration Build()
        {
            var spawner = new OpenGenericInstanceProvider(ImplementationType, Lifetime, Parameters);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }

        protected override void AddInterfaceType(Type interfaceType)
        {
            if (interfaceType.IsConstructedGenericType)
                throw new VContainerException(interfaceType, "Type is not open generic type.");

            foreach (var i in ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericTypeDefinition() != interfaceType)
                    continue;

                InterfaceTypes ??= new List<Type>();
                if (!InterfaceTypes.Contains(interfaceType))
                    InterfaceTypes.Add(interfaceType);

                return;
            }

            base.AddInterfaceType(interfaceType);
        }
    }
}