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
            if (!containerBuilder.Exists(typeof(EntryPointDispatcher), false))
            {
                containerBuilder.Register<EntryPointDispatcher>(Lifetime.Scoped);
                containerBuilder.RegisterBuildCallback(container =>
                {
                    container.Resolve<EntryPointDispatcher>().Dispatch();
                });
            }
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

        public static RegistrationBuilder RegisterEntryPoint<T>(this IContainerBuilder builder, Lifetime lifetime = Lifetime.Singleton)
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

        public static RegistrationBuilder RegisterComponent<TInterface>(this IContainerBuilder builder, TInterface component)
        {
            var registrationBuilder = new ComponentRegistrationBuilder(component).As(typeof(TInterface));
            // Force inject execution
            builder.RegisterBuildCallback(container => container.Resolve<TInterface>());
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentInHierarchy<T>(this IContainerBuilder builder)
        {
            var lifetimeScope = (LifetimeScope)builder.ApplicationOrigin;
            var scene = lifetimeScope.gameObject.scene;

            var registrationBuilder = new ComponentRegistrationBuilder(scene, typeof(T));
            // Force inject execution
            builder.RegisterBuildCallback(container =>
            {
                var type = registrationBuilder.InterfaceTypes != null
                    ? registrationBuilder.InterfaceTypes[0]
                    : registrationBuilder.ImplementationType;
                container.Resolve(type);
            });
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            string newGameObjectName = null)
            where T : Component
        {
            return builder.Register(new ComponentRegistrationBuilder(newGameObjectName, typeof(T), lifetime));
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            T prefab,
            Lifetime lifetime)
            where T : Component
        {
            return builder.Register(new ComponentRegistrationBuilder(prefab, typeof(T), lifetime));
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

        public static RegistrationBuilder RegisterSystemFromWorld<T>(this IContainerBuilder builder, World world)
            where T : ComponentSystemBase
        {
            var system = world.GetExistingSystem<T>();
            if (system is null)
                throw new ArgumentException($"{typeof(T).FullName} is not in the world {world}");

            return builder.RegisterComponent(system)
                .As(typeof(ComponentSystemBase), typeof(T));
        }

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

        public static SystemRegistrationBuilder RegisterSystemIntoDefaultWorld<T>(this IContainerBuilder builder)
            where T : ComponentSystemBase
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<SimulationSystemGroup>();

            return builder.Register(registrationBuilder);
        }
#endif
    }
}
