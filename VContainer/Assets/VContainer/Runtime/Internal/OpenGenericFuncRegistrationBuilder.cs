using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericFuncRegistrationBuilder : RegistrationBuilder
    {
        readonly Func<IObjectResolver, Type[], object> factory;

        public OpenGenericFuncRegistrationBuilder(
            Type openGenericType,
            Func<IObjectResolver, Type[], object> factory,
            Lifetime lifetime) : base(openGenericType, lifetime)
        {
            if (!openGenericType.IsGenericType || openGenericType.IsConstructedGenericType)
                throw new VContainerException(openGenericType, "Type is not open generic type.");
            
            this.factory = factory;
        }

        public override Registration Build()
        {
            var provider = new OpenGenericFuncInstanceProvider(ImplementationType, Lifetime, factory);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }

        public override RegistrationBuilder AsImplementedInterfaces()
        {
            InterfaceTypes ??= new List<Type>();
            foreach (var i in ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType)
                    continue;

                var def = i.GetGenericTypeDefinition();
                if (!InterfaceTypes.Contains(def))
                    InterfaceTypes.Add(def);
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

                InterfaceTypes ??= new List<Type>();

                if (!InterfaceTypes.Contains(interfaceType))
                    InterfaceTypes.Add(interfaceType);

                return;
            }

            base.AddInterfaceType(interfaceType);
        }
    }
}