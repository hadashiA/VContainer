using System;
using System.Runtime.CompilerServices;
using VContainer.Internal;

namespace VContainer
{
    public static class ContainerBuilderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime)
            => builder.Register(new RegistrationBuilder(type, lifetime));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime,
            Action<object, IObjectResolver> callback)
            => builder.Register(new RegistrationBuilderWithCallback(type, lifetime, callback));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<T>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            => builder.Register(typeof(T), lifetime);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<T, IObjectResolver> callback)
            => builder.Register(typeof(T), lifetime, (instance, resolver) => callback((T)instance, resolver));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface
            => builder.Register<TImplement>(lifetime).As<TInterface>();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<TImplement, IObjectResolver> callback)
            where TImplement : TInterface
            => builder.Register(lifetime, callback).As<TInterface>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2
            => builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<TImplement, IObjectResolver> callback)
            where TImplement : TInterface1, TInterface2
            => builder.Register(lifetime, callback).As(typeof(TInterface1), typeof(TInterface2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TInterface3, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2, TInterface3
            => builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TInterface3, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<TImplement, IObjectResolver> callback)
            where TImplement : TInterface1, TInterface2, TInterface3
            => builder.Register(lifetime, callback).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface>(
            this IContainerBuilder builder,
            Func<IObjectResolver, TInterface> implementationConfiguration,
            Lifetime lifetime)
            where TInterface : class
            => builder.Register(new FuncRegistrationBuilder(implementationConfiguration, typeof(TInterface), lifetime));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface>(
            this IContainerBuilder builder,
            Func<IObjectResolver, TInterface> implementationConfiguration,
            Lifetime lifetime,
            Action<TInterface, IObjectResolver> callback)
            where TInterface : class
            => builder.Register(new FuncRegistrationBuilderWithCallback(implementationConfiguration, typeof(TInterface), lifetime, (instance, resolver) => callback((TInterface)instance, resolver)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface>(
            this IContainerBuilder builder,
            TInterface instance)
            => builder.Register(new InstanceRegistrationBuilder(instance)).As(typeof(TInterface));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2, TInterface3>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<T>(
            this IContainerBuilder builder,
            Func<T> factory)
            => builder.RegisterInstance(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, T>(
            this IContainerBuilder builder,
            Func<TParam1, T> factory)
            => builder.RegisterInstance(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, T> factory)
            => builder.RegisterInstance(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, TParam3, T> factory)
            => builder.RegisterInstance(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, TParam4, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, TParam3, TParam4, T> factory)
            => builder.RegisterInstance(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<T>), lifetime));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, T>), lifetime));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, T>), lifetime));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, TParam3, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, TParam3, T>), lifetime));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, TParam4, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, TParam3, TParam4, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, TParam3, TParam4, T>), lifetime));

        [Obsolete("IObjectResolver is registered by default. This method does nothing.")]
        public static void RegisterContainer(this IContainerBuilder builder)
        {
        }
    }
}
