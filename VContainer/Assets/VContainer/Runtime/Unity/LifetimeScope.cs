using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class LifetimeScope : MonoBehaviour
    {
        public readonly struct ParentOverrideScope : IDisposable
        {
            public ParentOverrideScope(LifetimeScope nextParent) => OverrideParent = nextParent;
            public void Dispose() => OverrideParent = null;
        }

        public const string AutoReferenceKey = "__auto__";

        [SerializeField]
        MonoInstaller[] monoInstallers = {};

        [SerializeField]
        ScriptableObjectInstaller[] scriptableObjectInstallers = {};

        [SerializeField]
        string key = "";

        [SerializeField]
        public LifetimeScope parent;

        [SerializeField]
        public string parentKey;

        [SerializeField]
        bool autoRun = true;

        public const string ProjectRootResourcePath = "ProjectLifetimeScope";

        public static LifetimeScope ProjectRoot => ProjectRootLazy.Value;

        static readonly Lazy<LifetimeScope> ProjectRootLazy = new Lazy<LifetimeScope>(LoadProjectRoot);
        static readonly ConcurrentDictionary<string, ExtraInstaller> ExtraInstallers = new ConcurrentDictionary<string, ExtraInstaller>();
        static readonly List<GameObject> GameObjectBuffer = new List<GameObject>(32);

        static LifetimeScope OverrideParent;

        public static ParentOverrideScope PushParent(LifetimeScope parent) => new ParentOverrideScope(parent);

        public static ExtraInstallationScope Push(Action<IContainerBuilder> installing, string key = "")
            => new ExtraInstallationScope(new ActionInstaller(installing), key);

        public static ExtraInstallationScope Push(IInstaller installer, string key = "")
            => new ExtraInstallationScope(installer, key);

        public static LifetimeScope FindDefault(Scene scene) => FindByKey("", scene);

        public static LifetimeScope FindByKey(string key, Scene scene)
        {
            scene.GetRootGameObjects(GameObjectBuffer);
            foreach (var root in GameObjectBuffer)
            {
                var others = root.GetComponentsInChildren<LifetimeScope>();
                foreach (var other in others)
                {
                    if (key == other.key)
                    {
                        return other;
                    }
                }
            }
            return null;
        }

        public static LifetimeScope FindByKey(string key)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    var result = FindByKey(key, scene);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public static void EnqueueExtra(IInstaller installer, string key = "")
        {
            ExtraInstallers.AddOrUpdate(key,
                new ExtraInstaller { installer },
                (_, binding) =>
                {
                    binding.Add(installer);
                    return binding;
                });
        }

        public static void RemoveExtra(string key) => ExtraInstallers.TryRemove(key, out _);

        static LifetimeScope LoadProjectRoot()
        {
            var prefabs = Resources.LoadAll(ProjectRootResourcePath, typeof(GameObject));
            if (prefabs.Length > 1)
            {
                throw new VContainerException(null, $"{ProjectRootResourcePath} resource is duplicated!");
            }

            var prefab = (GameObject)prefabs.FirstOrDefault();
            if (prefab == null)
            {
                return null;
            }

            if (prefab.activeSelf)
            {
                prefab.SetActive(false);
            }
            var gameObject = Instantiate(prefab);
            DontDestroyOnLoad(gameObject);
            return gameObject.GetComponent<LifetimeScope>();
        }

        public IObjectResolver Container { get; private set; }
        public LifetimeScope Parent { get; private set; }
        public string Key => key;

        readonly CompositeDisposable disposable = new CompositeDisposable();
        readonly List<IInstaller> extraInstallers = new List<IInstaller>();


        void Awake()
        {
          Parent = GetRuntimeParent();
            if (autoRun)
            {
                Build();
            }
        }

        void OnDestroy()
        {
            disposable.Dispose();
            Container?.Dispose();
        }

        public void Build()
        {
            if (Parent == null)
            {
                Parent = GetRuntimeParent();
            }

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

        public LifetimeScope CreateChild(string childKey = null, IInstaller installer = null)
        {
            var childGameObject = new GameObject("LifeTimeScope (Child)");
            childGameObject.SetActive(false);
            childGameObject.transform.SetParent(transform, false);
            var child = childGameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                child.extraInstallers.Add(installer);
            }
            child.parent = this;
            child.key = childKey;
            childGameObject.SetActive(true);
            return child;
        }

        public LifetimeScope CreateChild(string childKey = null, Action<IContainerBuilder> installation = null)
        {
            if (installation != null)
            {
                return CreateChild(childKey, new ActionInstaller(installation));
            }
            return CreateChild(childKey, (IInstaller)null);
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
            child.parent = this;
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
            return child;
        }

        public LifetimeScope CreateChildFromPrefab(LifetimeScope prefab, Action<IContainerBuilder> installation = null)
        {
            if (installation != null)
            {
                return CreateChildFromPrefab(prefab, new ActionInstaller(installation));
            }
            return CreateChildFromPrefab(prefab, (IInstaller)null);
        }

        void InstallTo(IContainerBuilder builder)
        {
            foreach (var installer in monoInstallers)
            {
                installer.Install(builder);
            }

            foreach (var installer in scriptableObjectInstallers)
            {
                installer.Install(builder);
            }

            foreach (var installer in extraInstallers)
            {
                installer.Install(builder);
            }

            if (ExtraInstallers.TryRemove(key, out var extraInstaller))
            {
                extraInstaller.Install(builder);
            }

            Container = builder.Build();
        }

        LifetimeScope GetRuntimeParent()
        {
            var nextParent = OverrideParent;
            if (nextParent != null)
                return nextParent;

            if (parent != null)
                return parent;

            if (!string.IsNullOrEmpty(parentKey) && parentKey != key)
            {
                var found = FindByKey(parentKey);
                if (found != null)
                {
                    return found;
                }
                throw new VContainerException(null, $"LifetimeScope parent key `{parentKey}` is not found in any scene");
            }

            if (ProjectRoot != null && ProjectRoot != this)
            {
                if (!ProjectRoot.gameObject.activeSelf)
                {
                    ProjectRoot.gameObject.SetActive(true);
                }
                return ProjectRoot;
            }
            return null;
        }

        void DispatchPlayerLoopItems()
        {
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
        }
    }
}
