using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericRegistrationBuilder : RegistrationBuilder
    {
        public OpenGenericRegistrationBuilder(Type implementationType, Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            if (!implementationType.IsGenericType || implementationType.IsConstructedGenericType)
                throw new VContainerException(implementationType, "Type is not open generic type.");
        }

        public override Registration Build()
        {
            var provider = new OpenGenericInstanceProvider(ImplementationType, Lifetime, Parameters);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }

        public override RegistrationBuilder AsImplementedInterfaces()
        {
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            foreach (var i in ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType)
                    continue;

                InterfaceTypes.Add(i.GetGenericTypeDefinition());
            }
            return this;
        }

        protected override void AddInterfaceType(Type interfaceType)
        {
            if (interfaceType.IsConstructedGenericType)
                throw new VContainerException(interfaceType, "Type is not open generic type.");

            foreach (var i in ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericTypeDefinition() != interfaceType)
                    continue;

                if (InterfaceTypes is null)
                {
                    InterfaceTypes = new List<Type>();
                }

                if (!InterfaceTypes.Contains(interfaceType))
                    InterfaceTypes.Add(interfaceType);

                return;
            }

            base.AddInterfaceType(interfaceType);
        }
    }
}