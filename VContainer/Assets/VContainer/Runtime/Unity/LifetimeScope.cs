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
    public class LifetimeScope : MonoBehaviour, IDisposable
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

        [SerializeField]
        protected List<GameObject> autoInjectGameObjects;

        static LifetimeScope overrideParent;
        static ExtraInstaller extraInstaller;
        static readonly object SyncRoot = new object();

        static LifetimeScope Create(IInstaller installer = null)
        {
            var gameObject = new GameObject("LifeTimeScope");
            gameObject.SetActive(false);
            var newScope = gameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                newScope.extraInstallers.Add(installer);
            }
            gameObject.SetActive(true);
            return newScope;
        }

        public static LifetimeScope Create(Action<IContainerBuilder> installation)
            => Create(new ActionInstaller(installation));

        public static ParentOverrideScope EnqueueParent(LifetimeScope parent)
            => new ParentOverrideScope(parent);

        public static ExtraInstallationScope Enqueue(Action<IContainerBuilder> installing)
            => new ExtraInstallationScope(new ActionInstaller(installing));

        public static ExtraInstallationScope Enqueue(IInstaller installer)
            => new ExtraInstallationScope(installer);

        [Obsolete("LifetimeScope.PushParent is obsolete. Use LifetimeScope.EnqueueParent instead.", false)]
        public static ParentOverrideScope PushParent(LifetimeScope parent) => new ParentOverrideScope(parent);

        [Obsolete("LifetimeScope.Push is obsolete. Use LifetimeScope.Enqueue instead.", false)]
        public static ExtraInstallationScope Push(Action<IContainerBuilder> installing) => Enqueue(installing);

        [Obsolete("LifetimeScope.Push is obsolete. Use LifetimeScope.Enqueue instead.", false)]
        public static ExtraInstallationScope Push(IInstaller installer) => Enqueue(installer);

        public static LifetimeScope Find<T>(Scene scene) where T : LifetimeScope => Find(typeof(T), scene);
        public static LifetimeScope Find<T>() where T : LifetimeScope => Find(typeof(T));

        static LifetimeScope Find(Type type, Scene scene)
        {
            var buffer = UnityEngineObjectListBuffer<GameObject>.Get();
            scene.GetRootGameObjects(buffer);
            foreach (var gameObject in buffer)
            {
                var found = gameObject.GetComponentInChildren(type) as LifetimeScope;
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
            Container = null;
        }

        protected virtual void Configure(IContainerBuilder builder) { }

        public void Dispose()
        {
            if (this != null)
                Destroy(gameObject);
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
                var builder = new ContainerBuilder { ApplicationOrigin = this };
                InstallTo(builder);
                Container = builder.Build();
            }

            extraInstallers.Clear();
            AutoInjectAll();
            ActivateEntryPoints();
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
            => CreateChild(new ActionInstaller(installation));

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
            => CreateChildFromPrefab(prefab, new ActionInstaller(installation));

        void InstallTo(IContainerBuilder builder)
        {
            Configure(builder);

            foreach (var installer in extraInstallers)
            {
                installer.Install(builder);
            }

            ExtraInstaller extraInstallerStatic;
            lock (SyncRoot)
            {
                extraInstallerStatic = LifetimeScope.extraInstaller;
            }
            extraInstallerStatic?.Install(builder);

            builder.RegisterInstance<LifetimeScope>(this).AsSelf();
            builder.RegisterContainer();
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

        void AutoInjectAll()
        {
            if (autoInjectGameObjects == null)
                return;

            foreach (var target in autoInjectGameObjects)
            {
                if (target != null) // Check missing reference
                {
                    Container.InjectGameObject(target);
                }
            }
        }

        void ActivateEntryPoints()
        {
            PlayerLoopHelper.Initialize();

            var initializables = Container.Resolve<IReadOnlyList<IInitializable>>();
            if (initializables.Count > 0)
            {
                var loopItem = new InitializationLoopItem(initializables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Initialization, loopItem);
            }

            var postInitializables = Container.Resolve<IReadOnlyList<IPostInitializable>>();
            if (postInitializables.Count > 0)
            {
                var loopItem = new PostInitializationLoopItem(postInitializables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostInitialization, loopItem);
            }

            var startables = Container.Resolve<IReadOnlyList<IStartable>>();
            if (startables.Count > 0)
            {
                var loopItem = new StartableLoopItem(startables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }

            var postStartables = Container.Resolve<IReadOnlyList<IPostStartable>>();
            if (postStartables.Count > 0)
            {
                var loopItem = new PostStartableLoopItem(postStartables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostStartup, loopItem);
            }

            var fixedTickables = Container.Resolve<IReadOnlyList<IFixedTickable>>();
            if (fixedTickables.Count > 0)
            {
                var loopItem = new FixedTickableLoopItem(fixedTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }

            var postFixedTickables = Container.Resolve<IReadOnlyList<IPostFixedTickable>>();
            if (postFixedTickables.Count > 0)
            {
                var loopItem = new PostFixedTickableLoopItem(postFixedTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }

            var tickables = Container.Resolve<IReadOnlyList<ITickable>>();
            if (tickables.Count > 0)
            {
                var loopItem = new TickableLoopItem(tickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }

            var postTickables = Container.Resolve<IReadOnlyList<IPostTickable>>();
            if (postTickables.Count > 0)
            {
                var loopItem = new PostTickableLoopItem(postTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }

            var lateTickables = Container.Resolve<IReadOnlyList<ILateTickable>>();
            if (lateTickables.Count > 0)
            {
                var loopItem = new LateTickableLoopItem(lateTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }

            var postLateTickables = Container.Resolve<IReadOnlyList<IPostLateTickable>>();
            if (postLateTickables.Count > 0)
            {
                var loopItem = new PostLateTickableLoopItem(postLateTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }

#if VCONTAINER_UNITASK_INTEGRATION
            var asyncStartables = Container.Resolve<IReadOnlyList<IAsyncStartable>>();
            if (asyncStartables.Count > 0)
            {
                var loopItem = new AsyncStartableLoopItem(asyncStartables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }
#endif

#if VCONTAINER_ECS_INTEGRATION
            Container.Resolve<IEnumerable<ComponentSystemBase>>();

            var worldHelpers = Container.Resolve<IEnumerable<WorldConfigurationHelper>>();
            foreach (var x in worldHelpers)
            {
                x.SortSystems();
            }
#endif
        }
    }
}
