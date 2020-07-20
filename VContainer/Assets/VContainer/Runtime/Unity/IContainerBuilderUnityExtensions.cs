using System;
using UnityEngine;

#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;
#endif

namespace VContainer.Unity
{
    public readonly struct EntryPointsBuilder
    {
        readonly IContainerBuilder containerBuilder;
        readonly Lifetime lifetime;

        public EntryPointsBuilder(IContainerBuilder containerBuilder, Lifetime lifetime)
        {
            this.containerBuilder = containerBuilder;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder Add<T>()
            => containerBuilder.Register<T>(lifetime).AsImplementedInterfaces();
    }

    public readonly struct ComponentsBuilder
    {
        readonly IContainerBuilder containerBuilder;

        public ComponentsBuilder(IContainerBuilder containerBuilder)
        {
            this.containerBuilder = containerBuilder;
        }

        public RegistrationBuilder AddInstance(MonoBehaviour component)
            => containerBuilder.RegisterComponent(component);

        public RegistrationBuilder AddInHierarchy<T>() where T : MonoBehaviour
            => containerBuilder.RegisterComponentInHierarchy<T>();

        public ComponentRegistrationBuilder AddOnNewGameObject<T>(Lifetime lifetime, string newGameObjectName = null)
            where T : MonoBehaviour
            => containerBuilder.RegisterComponentOnNewGameObject<T>(lifetime, newGameObjectName);

        public ComponentRegistrationBuilder AddInNewPrefab<T>(T prefab, Lifetime lifetime)
            where T : MonoBehaviour
            => containerBuilder.RegisterComponentInNewPrefab(prefab, lifetime);
    }

    public static class IContainerBuilderUnityExtensions
    {
        public static void UseEntryPoints(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<EntryPointsBuilder> configuration)
        {
            var entryPoints = new EntryPointsBuilder(builder, lifetime);
            configuration(entryPoints);
        }

        public static void UseComponents(
            this IContainerBuilder builder,
            Action<ComponentsBuilder> configuration)
        {
            var components = new ComponentsBuilder(builder);
            configuration(components);
        }

        public static RegistrationBuilder RegisterEntryPoint<T>(
            this IContainerBuilder builder,
            Lifetime lifetime)
        {
            var registrationBuilder = builder.Register<T>(lifetime);
            if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                registrationBuilder = registrationBuilder.As(typeof(MonoBehaviour));
            }
            return registrationBuilder.AsImplementedInterfaces();
        }

        public static RegistrationBuilder RegisterComponent(
            this IContainerBuilder builder,
            MonoBehaviour component)
            => builder.RegisterInstance(component).As(typeof(MonoBehaviour), component.GetType());

        public static RegistrationBuilder RegisterComponent<TInterface>(
            this IContainerBuilder builder,
            TInterface component)
            => builder.RegisterInstance(component).As(typeof(MonoBehaviour), typeof(TInterface));

        public static RegistrationBuilder RegisterComponentInHierarchy<T>(
            this IContainerBuilder builder
            ) where T : MonoBehaviour
        {
            var lifetimeScope = (LifetimeScope)builder.ApplicationOrigin;
            var scene = lifetimeScope.gameObject.scene;
            var component = default(T);
            foreach (var x in scene.GetRootGameObjects())
            {
                component = x.GetComponentInChildren<T>(true);
                if (component != null) break;
            }

            if (component == null)
            {
                throw new VContainerException(typeof(T), $"Component {typeof(T)} is not in this scene {scene.path}");
            }

            return builder.RegisterInstance(component).As(typeof(MonoBehaviour), typeof(T));
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            string newGameObjectName = null)
            where T : MonoBehaviour
        {
            var registrationBuilder = new ComponentRegistrationBuilder(
                newGameObjectName,
                typeof(T),
                lifetime);
            builder.Register(registrationBuilder);
            return registrationBuilder;
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            T prefab,
            Lifetime lifetime)
            where T : MonoBehaviour
        {
            var registrationBuilder = new ComponentRegistrationBuilder(
                prefab,
                typeof(T),
                lifetime);
            builder.Register(registrationBuilder);
            return registrationBuilder;
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

            return builder.RegisterInstance(system)
                .As(typeof(ComponentSystemBase))
                .AsSelf();
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

            builder.Register(registrationBuilder);
            return registrationBuilder;
        }

        public static SystemRegistrationBuilder RegisterSystemIntoDefaultWorld<T>(this IContainerBuilder builder)
            where T : ComponentSystemBase
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), null)
                .IntoGroup<SimulationSystemGroup>();

            builder.Register(registrationBuilder);
            return registrationBuilder;
        }
#endif
    }
}
