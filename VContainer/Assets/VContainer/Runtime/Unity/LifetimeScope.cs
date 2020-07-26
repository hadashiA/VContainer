using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Internal;

#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;
#endif

namespace VContainer.Unity
{

    public class LifetimeScope : MonoBehaviour
    {
        public readonly struct ParentOverrideScope : IDisposable
        {
            public ParentOverrideScope(LifetimeScope nextParent) => overrideParent = nextParent;
            public void Dispose() => overrideParent = null;
        }

        public readonly struct ExtraInstallationScope : IDisposable
        {
            public ExtraInstallationScope(IInstaller installer) => EnqueueExtra(installer);
            void IDisposable.Dispose() => RemoveExtra();
        }

        [SerializeField]
        public ParentReference parentReference;

        [SerializeField]
        bool autoRun = true;

        static readonly List<GameObject> GameObjectBuffer = new List<GameObject>(32);

        static LifetimeScope overrideParent;
        static ExtraInstaller extraInstaller;
        static readonly object SyncRoot = new object();

        public static ParentOverrideScope PushParent(LifetimeScope parent) => new ParentOverrideScope(parent);

        public static ExtraInstallationScope Push(Action<IContainerBuilder> installing)
            => new ExtraInstallationScope(new ActionInstaller(installing));

        public static ExtraInstallationScope Push(IInstaller installer)
            => new ExtraInstallationScope(installer);

        public static LifetimeScope Find<T>(Scene scene) where T : LifetimeScope
            => Find(typeof(T), scene);

        public static LifetimeScope Find<T>() where T : LifetimeScope
            => Find(typeof(T));

        static LifetimeScope Find(Type type, Scene scene)
        {
            scene.GetRootGameObjects(GameObjectBuffer);
            foreach (var root in GameObjectBuffer)
            {
                var found = root.GetComponentInChildren(type) as LifetimeScope;
                if (found != null)
                    return found;
            }
            return null;
        }

        static LifetimeScope Find(Type type)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    var result = Find(type, scene);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        static void EnqueueExtra(IInstaller installer)
        {
            lock (SyncRoot)
            {
                if (extraInstaller != null)
                    extraInstaller.Add(installer);
                else
                    extraInstaller = new ExtraInstaller { installer };
            }
        }

        static void RemoveExtra()
        {
            lock (SyncRoot) extraInstaller = null;
        }

        public IObjectResolver Container { get; private set; }
        public LifetimeScope Parent { get; private set; }
        public bool IsRoot { get; set; }

        readonly CompositeDisposable disposable = new CompositeDisposable();
        readonly List<IInstaller> extraInstallers = new List<IInstaller>();

        protected virtual void Awake()
        {
            Parent = GetRuntimeParent();
            if (autoRun)
            {
                Build();
            }
        }

        protected virtual void OnDestroy()
        {
            disposable.Dispose();
            Container?.Dispose();
        }

        protected virtual void Configure(IContainerBuilder builder)
        {
        }

        public void Build()
        {
            if (Parent == null)
                Parent = GetRuntimeParent();

            if (Parent != null)
            {
                Container = Parent.Container.CreateScope(builder =>
                {
                    builder.ApplicationOrigin = this;
                    InstallTo(builder);
                });
            }
            else
            {
                InstallTo(new ContainerBuilder { ApplicationOrigin = this });
            }

            DispatchPlayerLoopItems();
        }

        public LifetimeScope CreateChild(IInstaller installer = null)
        {
            var childGameObject = new GameObject("LifeTimeScope (Child)");
            childGameObject.SetActive(false);
            childGameObject.transform.SetParent(transform, false);
            var child = childGameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                child.extraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            childGameObject.SetActive(true);
            return child;
        }

        public LifetimeScope CreateChild(Action<IContainerBuilder> installation)
        {
            if (installation != null)
            {
                return CreateChild(new ActionInstaller(installation));
            }
            return CreateChild();
        }

        public LifetimeScope CreateChildFromPrefab(LifetimeScope prefab, IInstaller installer = null)
        {
            if (prefab.gameObject.activeSelf && prefab.autoRun)
            {
                prefab.gameObject.SetActive(false);
            }

            var child = Instantiate(prefab, transform, false);
            if (installer != null)
            {
                child.extraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
            return child;
        }

        public LifetimeScope CreateChildFromPrefab(LifetimeScope prefab, Action<IContainerBuilder> installation)
        {
            if (installation != null)
            {
                return CreateChildFromPrefab(prefab, new ActionInstaller(installation));
            }
            return CreateChildFromPrefab(prefab);
        }

        void InstallTo(IContainerBuilder builder)
        {
            Configure(builder);

            foreach (var installer in extraInstallers)
            {
                installer.Install(builder);
            }

            ExtraInstaller extraInstaller;
            lock (SyncRoot)
            {
                extraInstaller = LifetimeScope.extraInstaller;
            }
            extraInstaller?.Install(builder);
            Container = builder.Build();
        }

        LifetimeScope GetRuntimeParent()
        {
            if (IsRoot) return null;

            var nextParent = overrideParent;
            if (nextParent != null)
                return nextParent;

            if (parentReference.Object != null)
                return parentReference.Object;

            if (parentReference.Type != null && parentReference.Type != GetType())
            {
                var found = Find(parentReference.Type);
                if (found != null)
                {
                    return found;
                }
                throw new VContainerException(null, $"LifetimeScope parent `{parentReference.Type.FullName}` is not found in any scene");
            }

            if (VContainerSettings.Instance is VContainerSettings settings)
            {
                if (settings.RootLifetimeScope != null)
                {
                    if (settings.RootLifetimeScope.Container == null)
                    {
                        settings.RootLifetimeScope.Build();
                    }
                    return settings.RootLifetimeScope;
                }
            }
            return null;
        }

        void DispatchPlayerLoopItems()
        {
            PlayerLoopHelper.Initialize();

            try
            {
                var markers = Container.Resolve<IEnumerable<IInitializable>>();
                var loopItem = new InitializationLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Initialization, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IInitializable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<IPostInitializable>>();
                var loopItem = new PostInitializationLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostInitialization, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IPostInitializable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<IFixedTickable>>();
                var loopItem = new FixedTickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IFixedTickable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<IPostFixedTickable>>();
                var loopItem = new PostFixedTickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IPostFixedTickable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<ITickable>>();
                var loopItem = new TickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<ITickable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<IPostTickable>>();
                var loopItem = new PostTickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IPostTickable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<ILateTickable>>();
                var loopItem = new LateTickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<ILateTickable>))
            {
            }

            try
            {
                var markers = Container.Resolve<IEnumerable<IPostLateTickable>>();
                var loopItem = new PostLateTickableLoopItem(markers);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<IPostLateTickable>))
            {
            }

            try
            {
                var _ = Container.Resolve<IEnumerable<MonoBehaviour>>();
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<MonoBehaviour>))
            {
            }

#if VCONTAINER_ECS_INTEGRATION
            try
            {
                var _ = Container.Resolve<IEnumerable<ComponentSystemBase>>();
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<ComponentSystemBase>))
            {
            }

            try
            {
                var worldHelpers = Container.Resolve<IEnumerable<WorldConfigurationHelper>>();
                foreach (var x in worldHelpers)
                {
                    x.SortSystems();
                }
            }
            catch (VContainerException ex) when(ex.InvalidType == typeof(IEnumerable<WorldConfigurationHelper>))
            {
            }
#endif
        }
    }
}
