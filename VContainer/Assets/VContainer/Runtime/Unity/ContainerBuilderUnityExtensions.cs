using System;
using UnityEngine;
using VContainer.Internal;

#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;
#endif

namespace VContainer.Unity
{
    public readonly struct EntryPointsBuilder
    {
        public static void EnsureDispatcherRegistered(IContainerBuilder containerBuilder)
        {
            if (containerBuilder.Exists(typeof(EntryPointDispatcher), false)) return;
            containerBuilder.Register<EntryPointDispatcher>(Lifetime.Scoped);
            containerBuilder.RegisterBuildCallback(container =>
            {
                container.Resolve<EntryPointDispatcher>().Dispatch();
            });
        }

        readonly IContainerBuilder containerBuilder;
        readonly Lifetime lifetime;

        public EntryPointsBuilder(IContainerBuilder containerBuilder, Lifetime lifetime)
        {
            this.containerBuilder = containerBuilder;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder Add<T>()
            => containerBuilder.Register<T>(lifetime).AsImplementedInterfaces();

        public void OnException(Action<Exception> exceptionHandler)
            => containerBuilder.RegisterEntryPointExceptionHandler(exceptionHandler);
    }

    public readonly struct ComponentsBuilder
    {
        readonly IContainerBuilder containerBuilder;
        readonly Transform parentTransform;

        public ComponentsBuilder(IContainerBuilder containerBuilder, Transform parentTransform = null)
        {
            this.containerBuilder = containerBuilder;
            this.parentTransform = parentTransform;
        }

        public RegistrationBuilder AddInstance<TInterface>(TInterface component)
        {
            return containerBuilder.RegisterComponent(component);
        }

        public ComponentRegistrationBuilder AddInHierarchy<T>()
            => containerBuilder.RegisterComponentInHierarchy<T>()
                .UnderTransform(parentTransform);

        public ComponentRegistrationBuilder AddOnNewGameObject<T>(Lifetime lifetime, string newGameObjectName = null)
            where T : Component
            => containerBuilder.RegisterComponentOnNewGameObject<T>(lifetime, newGameObjectName)
                .UnderTransform(parentTransform);

        public ComponentRegistrationBuilder AddInNewPrefab<T>(T prefab, Lifetime lifetime)
            where T : Component
            => containerBuilder.RegisterComponentInNewPrefab(prefab, lifetime)
                .UnderTransform(parentTransform);
    }

    public static class ContainerBuilderUnityExtensions
    {
        public static void UseEntryPoints(
            this IContainerBuilder builder,
            Action<EntryPointsBuilder> configuration)
        {
            builder.UseEntryPoints(Lifetime.Singleton, configuration);
        }

        public static void UseEntryPoints(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<EntryPointsBuilder> configuration)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            configuration(new EntryPointsBuilder(builder, lifetime));
        }

        public static void UseComponents(this IContainerBuilder builder, Action<ComponentsBuilder> configuration)
        {
            configuration(new ComponentsBuilder(builder));
        }

        public static void UseComponents(
            this IContainerBuilder builder,
            Transform root,
            Action<ComponentsBuilder> configuration)
        {
            configuration(new ComponentsBuilder(builder, root));
        }

        public static RegistrationBuilder RegisterEntryPoint<T>(
            this IContainerBuilder builder,
            Lifetime lifetime = Lifetime.Singleton)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            return builder.Register<T>(lifetime).AsImplementedInterfaces();
        }

        public static void RegisterEntryPointExceptionHandler(
            this IContainerBuilder builder,
            Action<Exception> exceptionHandler)
        {
            builder.RegisterInstance(new EntryPointExceptionHandler(exceptionHandler));
        }

        public static RegistrationBuilder RegisterComponent<TInterface>(
            this IContainerBuilder builder,
            TInterface component)
        {
            var registrationBuilder = new ComponentRegistrationBuilder(component).As(typeof(TInterface));
            // Force inject execution
            builder.RegisterBuildCallback(container => container.Resolve<TInterface>());
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentInHierarchy(
            this IContainerBuilder builder,
            Type type)
        {
            var lifetimeScope = (LifetimeScope)builder.ApplicationOrigin;
            var scene = lifetimeScope.gameObject.scene;

            var registrationBuilder = new ComponentRegistrationBuilder(scene, type);
            // Force inject execution
            builder.RegisterBuildCallback(
                container =>
                {
                    container.Resolve(
                        registrationBuilder.InterfaceTypes != null
                            ? registrationBuilder.InterfaceTypes[0]
                            : registrationBuilder.ImplementationType
                    );
                }
            );
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentInHierarchy<T>(this IContainerBuilder builder)
        {
            return builder.RegisterComponentInHierarchy(typeof(T));
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime,
            string newGameObjectName = null)
        {
            return builder.Register(new ComponentRegistrationBuilder(newGameObjectName, type, lifetime));
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            string newGameObjectName = null)
            where T : Component
        {
            return builder.RegisterComponentOnNewGameObject(typeof(T), lifetime, newGameObjectName);
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab(
            this IContainerBuilder builder,
            Type interfaceType,
            Component prefab,
            Lifetime lifetime)
        {
            var componentRegistrationBuilder = builder.Register(new ComponentRegistrationBuilder(_ => prefab, prefab.GetType(), lifetime));
            componentRegistrationBuilder.As(interfaceType);
            return componentRegistrationBuilder;
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            T prefab,
            Lifetime lifetime)
            where T : Component
        {
            return builder.RegisterComponentInNewPrefab(typeof(T), prefab, lifetime);
        }
        
        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, T> prefab,
            Lifetime lifetime)
            where T : Component
        {
            return builder.Register(new ComponentRegistrationBuilder(prefab, typeof(T), lifetime));
        }
        
        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<TInterface, TImplement>(
            this IContainerBuilder builder,
            Func<IObjectResolver, TImplement> prefab,
            Lifetime lifetime)
            where TImplement : Component, TInterface
        {
            var componentRegistrationBuilder = builder.Register(new ComponentRegistrationBuilder(prefab, typeof(TImplement), lifetime));
            componentRegistrationBuilder.As<TInterface>();
            return componentRegistrationBuilder;
        }

#if VCONTAINER_ECS_INTEGRATION
        public readonly struct NewWorldBuilder
        {
            readonly IContainerBuilder containerBuilder;
            readonly string worldName;
            readonly Lifetime worldLifetime;

            public NewWorldBuilder(IContainerBuilder containerBuilder, string worldName, Lifetime worldLifetime)
            {
                this.containerBuilder = containerBuilder;
                this.worldName = worldName;
                this.worldLifetime = worldLifetime;

                containerBuilder.RegisterNewWorld(worldName, worldLifetime);
            }

            public SystemRegistrationBuilder Add<T>() where T : ComponentSystemBase
                => containerBuilder.RegisterSystemIntoWorld<T>(worldName);
#if UNITY_2022_2_OR_NEWER
            public UnmanagedSystemRegistrationBuilder AddUnmanaged<T>(T system) where T : unmanaged, ISystem
                => containerBuilder.RegisterUnmanagedSystemIntoWorld<T>(worldName);
#endif
        }

        public readonly struct DefaultWorldBuilder
        {
            readonly IContainerBuilder containerBuilder;

            public DefaultWorldBuilder(IContainerBuilder containerBuilder)
            {
                this.containerBuilder = containerBuilder;
            }

            public RegistrationBuilder Add<T>() where T : ComponentSystemBase
                => containerBuilder.RegisterSystemFromDefaultWorld<T>();
#if UNITY_2022_2_OR_NEWER
            public RegistrationBuilder AddUnmanaged<T>() where T : unmanaged, ISystem
                => containerBuilder.RegisterUnmanagedSystemFromDefaultWorld<T>();
#endif
        }

        // Use exisiting world

        public static void UseDefaultWorld(this IContainerBuilder builder, Action<DefaultWorldBuilder> configuration)
        {
            var systems = new DefaultWorldBuilder(builder);
            configuration(systems);
        }

        public static RegistrationBuilder RegisterSystemFromDefaultWorld<T>(this IContainerBuilder builder)
            where T : ComponentSystemBase
            => RegisterSystemFromWorld<T>(builder, World.DefaultGameObjectInjectionWorld);
#if UNITY_2022_2_OR_NEWER
        public static RegistrationBuilder RegisterUnmanagedSystemFromDefaultWorld<T>(this IContainerBuilder builder)
            where T : unmanaged, ISystem
            => RegisterUnmanagedSystemFromWorld<T>(builder, World.DefaultGameObjectInjectionWorld);
#endif
        public static RegistrationBuilder RegisterSystemFromWorld<T>(this IContainerBuilder builder, World world)
            where T : ComponentSystemBase
        {
#if UNITY_2022_2_OR_NEWER
            var system = world.GetExistingSystemManaged<T>();
#else
            var system = world.GetExistingSystem<T>();
#endif
            if (system is null)
                throw new ArgumentException($"{typeof(T).FullName} is not in the world {world}");

            return builder.RegisterComponent(system)
                .As(typeof(ComponentSystemBase), typeof(T));
        }
#if UNITY_2022_2_OR_NEWER
        public static RegistrationBuilder RegisterUnmanagedSystemFromWorld<T>(this IContainerBuilder builder, World world)
            where T : unmanaged, ISystem
        {
            var system = world.Unmanaged.GetExistingUnmanagedSystem<T>();
            if (system == SystemHandle.Null)
                throw new ArgumentException($"{typeof(T).FullName} is not in the world {world}");

            Type refType = typeof(UnmanagedSystemReference<>);
            Type target = refType.MakeGenericType(typeof(T));
            var reference = (UnmanagedSystemReference)Activator.CreateInstance(target, system, world);
            
            return builder.RegisterComponent(reference)
                .As(target);
        }
#endif
        // Use custom world

        public static void UseNewWorld(
            this IContainerBuilder builder,
            string worldName,
            Lifetime lifetime,
            Action<NewWorldBuilder> configuration)
        {
            var systems = new NewWorldBuilder(builder, worldName, lifetime);
            configuration(systems);
        }

        public static RegistrationBuilder RegisterNewWorld(
            this IContainerBuilder builder,
            string worldName,
            Lifetime lifetime,
            Action<World> configuration = null)
        {
            builder.Register<WorldConfigurationHelper>(lifetime)
                .WithParameter(typeof(string), worldName);
            return builder.Register(new WorldRegistrationBuilder(worldName, lifetime, configuration));
        }

        public static SystemRegistrationBuilder RegisterSystemIntoWorld<T>(
            this IContainerBuilder builder,
            string worldName)
            where T : ComponentSystemBase
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), worldName)
                .IntoGroup<SimulationSystemGroup>();

            return builder.Register(registrationBuilder);
        }
        public static SystemRegistrationBuilder RegisterSystemIntoWorld<T, T1>(
            this IContainerBuilder builder,
            string worldName)
            where T : ComponentSystemBase
            where T1 :　ComponentSystemGroup
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), worldName)
                .IntoGroup<T1>();

            return builder.Register(registrationBuilder);
        }

#if UNITY_2022_2_OR_NEWER
        public static UnmanagedSystemRegistrationBuilder RegisterUnmanagedSystemIntoWorld<T>(
            this IContainerBuilder builder,
            string worldName)
            where T : unmanaged, ISystem
        {
            var registrationBuilder = new UnmanagedSystemRegistrationBuilder(typeof(T), worldName)
                .IntoGroup<SimulationSystemGroup>();

            return builder.Register(registrationBuilder);
        }

        public static UnmanagedSystemRegistrationBuilder RegisterUnmanagedSystemIntoWorld<T, T1>(
            this IContainerBuilder builder,
            string worldName)
            where T : unmanaged, ISystem
            where T1 :　ComponentSystemGroup
        {
            var registrationBuilder = new UnmanagedSystemRegistrationBuilder(typeof(T), worldName)
                .IntoGroup<T1>();

            return builder.Register(registrationBuilder);
        }
#endif
        public static SystemRegistrationBuilder RegisterSystemIntoDefaultWorld<T>(this IContainerBuilder builder)
            where T : ComponentSystemBase
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<SimulationSystemGroup>();

            return builder.Register(registrationBuilder);
        }

        public static SystemRegistrationBuilder RegisterSystemIntoDefaultWorld<T, T1>(this IContainerBuilder builder)
            where T : ComponentSystemBase
            where T1 :　ComponentSystemGroup
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<T1>();

            return builder.Register(registrationBuilder);
        }

#if UNITY_2022_2_OR_NEWER
        public static UnmanagedSystemRegistrationBuilder RegisterUnmanagedSystemIntoDefaultWorld<T>(this IContainerBuilder builder)
            where T : unmanaged, ISystem
        {
            var registrationBuilder = new UnmanagedSystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<SimulationSystemGroup>();

            return builder.Register(registrationBuilder);
        }
        public static UnmanagedSystemRegistrationBuilder RegisterUnmanagedSystemIntoDefaultWorld<T, T1>(this IContainerBuilder builder)
            where T : unmanaged, ISystem
            where T1 :　ComponentSystemGroup
        {
            var registrationBuilder = new UnmanagedSystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<T1>();

            return builder.Register(registrationBuilder);
        }
#endif
#endif
    }
}
