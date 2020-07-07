using System;
using UnityEngine;

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

    }
}