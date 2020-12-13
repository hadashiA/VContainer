using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class FactoryRegistration<T> : RegistrationBuilder, IRegistration
    {
        readonly Func<IObjectResolver, Func<T>> factoryFactory;

        internal FactoryRegistration(Func<IObjectResolver, Func<T>> factoryFactory, Lifetime lifetime)
            : base(typeof(Func<T>), lifetime)
        {
            this.factoryFactory = factoryFactory;
        }

        public new Type ImplementationType => base.ImplementationType;
        public new IReadOnlyList<Type> InterfaceTypes => null;
        public new Lifetime Lifetime => base.Lifetime;

        public override IRegistration Build() => this;
        public object SpawnInstance(IObjectResolver resolver) => factoryFactory(resolver);
    }

    sealed class FactoryRegistration<TParam, T> : RegistrationBuilder, IRegistration
    {
        readonly Func<IObjectResolver, Func<TParam, T>> factoryFactory;

        internal FactoryRegistration(Func<IObjectResolver, Func<TParam, T>> factoryFactory, Lifetime lifetime)
            : base(typeof(Func<TParam, T>), lifetime)
        {
            this.factoryFactory = factoryFactory;
        }

        public new Type ImplementationType => base.ImplementationType;
        public new IReadOnlyList<Type> InterfaceTypes => null;
        public new Lifetime Lifetime => base.Lifetime;

        public override IRegistration Build() => this;
        public object SpawnInstance(IObjectResolver resolver) => factoryFactory(resolver);
    }

    sealed class FactoryRegistration<TParam1, TParam2, T> : RegistrationBuilder, IRegistration
    {
        readonly Func<IObjectResolver, Func<TParam1, TParam2, T>> factoryFactory;

        internal FactoryRegistration(Func<IObjectResolver, Func<TParam1, TParam2, T>> factoryFactory, Lifetime lifetime)
            : base(typeof(Func<TParam1, TParam2, T>), lifetime)
        {
            this.factoryFactory = factoryFactory;
        }

        public new Type ImplementationType => base.ImplementationType;
        public new IReadOnlyList<Type> InterfaceTypes => null;
        public new Lifetime Lifetime => base.Lifetime;

        public override IRegistration Build() => this;
        public object SpawnInstance(IObjectResolver resolver) => factoryFactory(resolver);
    }

    sealed class FactoryRegistration<TParam1, TParam2, TParam3, T> : RegistrationBuilder, IRegistration
    {
        readonly Func<IObjectResolver, Func<TParam1, TParam2, TParam3, T>> factoryFactory;

        internal FactoryRegistration(Func<IObjectResolver, Func<TParam1, TParam2, TParam3, T>> factoryFactory, Lifetime lifetime)
            : base(typeof(Func<TParam1, TParam2, TParam3, T>), lifetime)
        {
            this.factoryFactory = factoryFactory;
        }

        public new Type ImplementationType => base.ImplementationType;
        public new IReadOnlyList<Type> InterfaceTypes => null;
        public new Lifetime Lifetime => base.Lifetime;

        public override IRegistration Build() => this;
        public object SpawnInstance(IObjectResolver resolver) => factoryFactory(resolver);
    }

    sealed class FactoryRegistration<TParam1, TParam2, TParam3, TParam4, T> : RegistrationBuilder, IRegistration
    {
        readonly Func<IObjectResolver, Func<TParam1, TParam2, TParam3, TParam4, T>> factoryFactory;

        internal FactoryRegistration(Func<IObjectResolver, Func<TParam1, TParam2, TParam3, TParam4, T>> factoryFactory, Lifetime lifetime)
            : base(typeof(Func<TParam1, TParam2, TParam3, TParam4, T>), lifetime)
        {
            this.factoryFactory = factoryFactory;
        }

        public new Type ImplementationType => base.ImplementationType;
        public new IReadOnlyList<Type> InterfaceTypes => null;
        public new Lifetime Lifetime => base.Lifetime;

        public override IRegistration Build() => this;
        public object SpawnInstance(IObjectResolver resolver) => factoryFactory(resolver);
    }
}
