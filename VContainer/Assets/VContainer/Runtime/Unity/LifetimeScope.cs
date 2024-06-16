using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Diagnostics;
using VContainer.Internal;

namespace VContainer.Unity
{
    [DefaultExecutionOrder(-5000)]
    public partial class LifetimeScope : MonoBehaviour, IDisposable
    {
        public readonly struct ParentOverrideScope : IDisposable
        {
            public ParentOverrideScope(LifetimeScope nextParent)
            {
                lock (SyncRoot)
                {
                    GlobalOverrideParents.Push(nextParent);
                }
            }

            public void Dispose()
            {
                lock (SyncRoot)
                {
                    GlobalOverrideParents.Pop();
                }
            }
        }

        public readonly struct ExtraInstallationScope : IDisposable
        {
            public ExtraInstallationScope(IInstaller installer)
            {
                lock (SyncRoot)
                {
                    GlobalExtraInstallers.Push(installer);
                }
            }

            void IDisposable.Dispose()
            {
                lock (SyncRoot)
                {
                    GlobalExtraInstallers.Pop();
                }
            }
        }

        [SerializeField]
        public ParentReference parentReference;

        [SerializeField]
        public bool autoRun = true;

        [SerializeField]
        protected List<GameObject> autoInjectGameObjects;

        string scopeName;

        static readonly Stack<LifetimeScope> GlobalOverrideParents = new Stack<LifetimeScope>();
        static readonly Stack<IInstaller> GlobalExtraInstallers = new Stack<IInstaller>();
        static readonly object SyncRoot = new object();

        static LifetimeScope Create(IInstaller installer = null)
        {
            var gameObject = new GameObject("LifetimeScope");
            gameObject.SetActive(false);
            var newScope = gameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                newScope.localExtraInstallers.Add(installer);
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
            using (ListPool<GameObject>.Get(out var buffer))
            {
                scene.GetRootGameObjects(buffer);
                foreach (var gameObject in buffer)
                {
                    var found = gameObject.GetComponentInChildren(type) as LifetimeScope;
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        static LifetimeScope Find(Type type)
        {
#if UNITY_2020_4_OR_NEWER || UNITY_2021_4_OR_NEWER || UNITY_2022_3_OR_NEWER || UNITY_2021_3_OR_NEWER
            return (LifetimeScope)FindAnyObjectByType(type);
#else
            return (LifetimeScope)FindObjectOfType(type);
#endif
        }

        public IObjectResolver Container { get; private set; }
        public LifetimeScope Parent { get; private set; }

        public bool IsRoot => VContainerSettings.Instance != null &&
                              VContainerSettings.Instance.IsRootLifetimeScopeInstance(this);

        readonly List<IInstaller> localExtraInstallers = new List<IInstaller>();

        protected virtual void Awake()
        {
            if (VContainerSettings.DiagnosticsEnabled && string.IsNullOrEmpty(scopeName))
            {
                scopeName = $"{name} ({gameObject.GetInstanceID()})";
            }
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
            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        public void DisposeCore()
        {
            Container?.Dispose();
            Container = null;
            CancelAwake(this);
            if (VContainerSettings.DiagnosticsEnabled)
            {
                DiagnositcsContext.RemoveCollector(scopeName);
            }
        }

        public void Build()
        {
            if (Parent == null)
                Parent = GetRuntimeParent();

            if (Parent != null)
            {
                if (VContainerSettings.Instance != null && Parent.IsRoot)
                {
                    if (Parent.Container == null)
                        Parent.Build();
                }

                // ReSharper disable once PossibleNullReferenceException
                Parent.Container.CreateScope(builder =>
                {
                    builder.RegisterBuildCallback(SetContainer);
                    builder.ApplicationOrigin = this;
                    builder.Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(scopeName) : null;
                    InstallTo(builder);
                });
            }
            else
            {
                var builder = new ContainerBuilder
                {
                    ApplicationOrigin = this,
                    Diagnostics = VContainerSettings.DiagnosticsEnabled ? DiagnositcsContext.GetCollector(scopeName) : null,
                };
                builder.RegisterBuildCallback(SetContainer);
                InstallTo(builder);
                builder.Build();
            }

            AwakeWaitingChildren(this);
        }

        void SetContainer(IObjectResolver container)
        {
            Container = container;
            AutoInjectAll();
        }

        public TScope CreateChild<TScope>(IInstaller installer = null)
            where TScope : LifetimeScope
        {
            var childGameObject = new GameObject("LifetimeScope (Child)");
            childGameObject.SetActive(false);
            if (IsRoot)
            {
                DontDestroyOnLoad(childGameObject);
            }
            else
            {
                childGameObject.transform.SetParent(transform, false);
            }
            var child = childGameObject.AddComponent<TScope>();
            if (installer != null)
            {
                child.localExtraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            childGameObject.SetActive(true);
            return child;
        }

        public LifetimeScope CreateChild(IInstaller installer = null)
            => CreateChild<LifetimeScope>(installer);

        public TScope CreateChild<TScope>(Action<IContainerBuilder> installation)
            where TScope : LifetimeScope
            => CreateChild<TScope>(new ActionInstaller(installation));

        public LifetimeScope CreateChild(Action<IContainerBuilder> installation)
            => CreateChild<LifetimeScope>(new ActionInstaller(installation));

        public TScope CreateChildFromPrefab<TScope>(TScope prefab, IInstaller installer = null)
            where TScope : LifetimeScope
        {
            var wasActive = prefab.gameObject.activeSelf;
            if (wasActive)
            {
                prefab.gameObject.SetActive(false);
            }
            var child = Instantiate(prefab, transform, false);
            if (installer != null)
            {
                child.localExtraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            if (wasActive)
            {
                prefab.gameObject.SetActive(true);
                child.gameObject.SetActive(true);
            }
            return child;
        }

        public TScope CreateChildFromPrefab<TScope>(TScope prefab, Action<IContainerBuilder> installation)
            where TScope : LifetimeScope
            => CreateChildFromPrefab(prefab, new ActionInstaller(installation));

        void InstallTo(IContainerBuilder builder)
        {
            Configure(builder);

            foreach (var installer in localExtraInstallers)
            {
                installer.Install(builder);
            }
            localExtraInstallers.Clear();

            lock (SyncRoot)
            {
                foreach (var installer in GlobalExtraInstallers)
                {
                    installer.Install(builder);
                }
            }

            builder.RegisterInstance<LifetimeScope>(this).AsSelf();
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
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

            lock (SyncRoot)
            {
                if (GlobalOverrideParents.Count > 0)
                {
                    return GlobalOverrideParents.Peek();
                }
            }

            // Find root from settings
            if (VContainerSettings.Instance != null)
            {
                return VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
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
