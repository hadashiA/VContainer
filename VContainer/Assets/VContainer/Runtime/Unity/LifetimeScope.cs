using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Diagnostics;

namespace VContainer.Unity
{
    [DefaultExecutionOrder(-5000)]
    public partial class LifetimeScope : MonoBehaviour, IDisposable
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
            var gameObject = new GameObject("LifetimeScope");
            gameObject.SetActive(false);
            var newScope = gameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                newScope.extraInstallers.Add(installer);
            }
            gameObject.SetActive(true);
            return newScope;
        }

        public static LifetimeScope Create(Action<IContainerBuilder> configuration)
            => Create(new ActionInstaller(configuration));

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
           return (LifetimeScope)FindObjectOfType(type);
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

        readonly List<IInstaller> extraInstallers = new List<IInstaller>();

        protected virtual void Awake()
        {
            try
            {
                Parent = GetRuntimeParent();
                if (autoRun)
                {
                    Build();
                }
            }
            catch (VContainerParentTypeReferenceNotFound) when(!IsRoot)
            {
                if (WaitingList.Contains(this))
                {
                    throw;
                }
                EnqueueAwake(this);
            }
        }

        protected virtual void OnDestroy()
        {
            DisposeCore();
        }

        protected virtual void Configure(IContainerBuilder builder) { }

        public void Dispose()
        {
            DisposeCore();
            if (this != null) Destroy(gameObject);
        }

        public void DisposeCore()
        {
            Container?.Dispose();
            Container = null;
            CancelAwake(this);
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
                    builder.Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(name) : null;
                    InstallTo(builder);
                });
            }
            else
            {
                var builder = new ContainerBuilder
                {
                    ApplicationOrigin = this,
                    Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(name) : null,
                };
                InstallTo(builder);
                Container = builder.Build();
            }

            extraInstallers.Clear();

            AutoInjectAll();
            AwakeWaitingChildren(this);
        }

        public LifetimeScope CreateChild(IInstaller installer = null)
        {
            var childGameObject = new GameObject("LifetimeScope (Child)");
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
            var wasActive = prefab.gameObject.activeSelf;
            if (wasActive)
            {
                prefab.gameObject.SetActive(false);
            }
            var child = Instantiate(prefab, transform, false);
            if (installer != null)
            {
                child.extraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            if (wasActive)
            {
                prefab.gameObject.SetActive(true);
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
        }

        LifetimeScope GetRuntimeParent()
        {
            if (IsRoot) return null;

            if (parentReference.Object != null)
                return parentReference.Object;

            // Find in scene via type
            if (parentReference.Type != null && parentReference.Type != GetType())
            {
                var found = Find(parentReference.Type);
                if (found != null && found.Container != null)
                {
                    return found;
                }
                throw new VContainerParentTypeReferenceNotFound(
                    parentReference.Type,
                    $"{name} could not found parent reference of type : {parentReference.Type}");
            }

            var nextParent = overrideParent;
            if (nextParent != null)
                return nextParent;

            // Find root from settings
            if (VContainerSettings.Instance != null)
            {
                var rootLifetimeScope = VContainerSettings.Instance.RootLifetimeScope;
                if (rootLifetimeScope != null)
                {
                    if (rootLifetimeScope.Container == null)
                    {
                        rootLifetimeScope.Build();
                    }
                    return rootLifetimeScope;
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
    }
}
