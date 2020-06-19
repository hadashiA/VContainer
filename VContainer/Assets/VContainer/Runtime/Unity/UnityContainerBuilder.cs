using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    // Decorate Unity features
    public sealed class UnityContainerBuilder : IContainerBuilder
    {
        readonly IContainerBuilder builder;
        Scene scene;

        GameObject[] RootGameObjects
        {
            get
            {
                if (rootGameObjects == null)
                    rootGameObjects = scene.GetRootGameObjects();
                return rootGameObjects;
            }
        }

        GameObject[] rootGameObjects;

        public UnityContainerBuilder(Scene scene)
        {
            builder = new ContainerBuilder();
            this.scene = scene;
        }

        public UnityContainerBuilder(IContainerBuilder builder, Scene scene)
        {
            this.builder = builder;
            this.scene = scene;
        }

        public RegistrationBuilder Register<T>(Lifetime lifetime) => builder.Register<T>(lifetime);
        public RegistrationBuilder RegisterInstance(object instance) => builder.RegisterInstance(instance);
        public RegistrationBuilder Register(RegistrationBuilder registrationBuilder) => builder.Register(registrationBuilder);

        public void RegisterContainer() => builder.RegisterContainer();

        public IObjectResolver Build() => builder.Build();

        public RegistrationBuilder RegisterComponentInHierarchy<T>() where T : Component
        {
            var component = default(T);
            foreach (var x in RootGameObjects)
            {
                component = x.GetComponentInChildren<T>(true);
                if (component != null) break;
            }

            if (component == null)
            {
                throw new VContainerException(typeof(T), $"Component {typeof(T)} is not in this scene {scene.path}");
            }

            return RegisterInstance(component);
        }

        public ComponentRegistrationBuilder RegisterComponentOnNewGameObject<T>(
            Lifetime lifetime,
            string newGameObjectName = null)
            where T : Component
        {
            var registrationBuilder = new ComponentRegistrationBuilder(
                newGameObjectName,
                typeof(T),
                lifetime);
            builder.Register(registrationBuilder);
            return registrationBuilder;
        }

        public ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(T prefab, Lifetime lifetime)
            where T : Component
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