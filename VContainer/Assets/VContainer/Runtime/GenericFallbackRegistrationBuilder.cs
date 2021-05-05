using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public sealed class GenericFallbackRegistrationBuilder : RegistrationBuilder
    {
        bool asImplementedInterfaces;
        bool asSelf;

        public GenericFallbackRegistrationBuilder(Type implementationType, Lifetime lifetime)
			: base(implementationType, lifetime)
        {
            if (!implementationType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"{implementationType} is not an open generic type");
            }
        }

        public new GenericFallbackRegistrationBuilder AsImplementedInterfaces()
        {
            asImplementedInterfaces = true;
            return this;
        }

        public new GenericFallbackRegistrationBuilder AsSelf()
        {
            asSelf = true;
            return this;
        }

        public override IRegistration Build()
        {
            return new GenericFallbackRegistration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                asImplementedInterfaces,
                asSelf);
        }
	}
}