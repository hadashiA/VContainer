using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    // Decorate Unity features
    public sealed class ContainerBuilderUnity : IContainerBuilder
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

        public ContainerBuilderUnity(Scene scene)
        {
            builder = new ContainerBuilder();
            this.scene = scene;
        }

        public ContainerBuilderUnity(IContainerBuilder builder, Scene scene)
        {
            this.builder = builder;
            this.scene = scene;
        }

        public RegistrationBuilder Register<T>(Lifetime lifetime) => builder.Register<T>(lifetime);
        public RegistrationBuilder RegisterInstance(object instance) => builder.RegisterInstance(instance);
        public void RegisterContainer() => builder.RegisterContainer();

        public IObjectResolver Build() => builder.Build();

        public RegistrationBuilder RegisterComponentInHierarchy<T>()
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
    }
}