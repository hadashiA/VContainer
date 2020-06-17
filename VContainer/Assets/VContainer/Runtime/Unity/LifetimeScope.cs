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
        [SerializeField]
        MonoInstaller[] monoInstallers;

        [SerializeField]
        ScriptableObjectInstaller[] scriptableObjectInstallers;

        [SerializeField]
        LifetimeScope parent;

        [SerializeField]
        string parentKey;

        [SerializeField]
        public string Key = "";

        const string ProjectRootResourcePath = "ProjectLifetimeScope";

        public static LifetimeScope ProjectRoot => ProjectRootLazy.Value;

        static readonly Lazy<LifetimeScope> ProjectRootLazy = new Lazy<LifetimeScope>(LoadProjectRoot);
        static readonly ConcurrentDictionary<string, ExtraInstaller> Extensions = new ConcurrentDictionary<string, ExtraInstaller>();

        public static ExtendScope BeginLoading(Action<ContainerBuilderUnity> installing, string key = "")
        {
            return new ExtendScope(new ActionInstaller(installing), key);
        }

        public static ExtendScope BeginLoading(IInstaller installer, string key = "")
        {
            return new ExtendScope(installer, key);
        }

        public static void EnqueueExtra(IInstaller installer, string key = "")
        {
            Extensions.AddOrUpdate(key,
                new ExtraInstaller { installer },
                (_, binding) =>
                {
                    binding.Add(installer);
                    return binding;
                });
        }

        public static void RemoveExtra(string key)
        {
            Extensions.TryRemove(key, out _);
        }

        static LifetimeScope LoadProjectRoot()
        {
            UnityEngine.Debug.Log($"VContainer try to load project root: {ProjectRootResourcePath}");
            var prefabs = Resources.LoadAll(ProjectRootResourcePath, typeof(GameObject));
            if (prefabs.Length > 1)
            {
                throw new VContainerException($"{ProjectRootResourcePath} resource is duplicated!");
            }

            var prefab = prefabs.FirstOrDefault();
            if (prefab == null)
            {
                return null;
            }

            var gameObject = (GameObject)Instantiate(prefab);
            DontDestroyOnLoad(gameObject);
            return gameObject.GetComponent<LifetimeScope>();
        }

        public IObjectResolver Container { get; private set; }

        readonly CompositeDisposable disposable = new CompositeDisposable();
        readonly List<GameObject> gameObjectBuffer = new List<GameObject>(32);

        void Awake()
        {
            if (GetRuntimeParent() is LifetimeScope parentScope)
            {
                Container = parentScope.Container.CreateScope(InstallTo);
            }
            else
            {
                InstallTo(new ContainerBuilder());
            }

            DispatchPlayerLoopItems();
        }

        void OnDestroy()
        {
            disposable.Dispose();
            Container.Dispose();
        }

        public void CreateScopeFromPrefab(
            LifetimeScope childPrefab,
            Action<ContainerBuilderUnity> configuration = null,
            string lookUpKey = null)
        {
            if (childPrefab.gameObject.activeSelf)
            {
                childPrefab.gameObject.SetActive(false);
            }
            var child = Instantiate(childPrefab, transform, false);
            child.parent = this;
            child.Key = lookUpKey;
            child.gameObject.SetActive(true);
        }

        void InstallTo(IContainerBuilder builder)
        {
            var decoratedBuilder = new ContainerBuilderUnity(builder, this);
            foreach (var installer in monoInstallers)
            {
                installer.Install(decoratedBuilder);
            }

            foreach (var installer in scriptableObjectInstallers)
            {
                installer.Install(decoratedBuilder);
            }

            if (Extensions.TryRemove(Key, out var extraInstaller))
            {
                extraInstaller.Install(decoratedBuilder);
            }
        }

        LifetimeScope GetRuntimeParent()
        {
            if (parent != null)
                return parent;

            if (parentKey != null && parentKey != Key)
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded)
                    {
                        gameObjectBuffer.Clear();
                        scene.GetRootGameObjects(gameObjectBuffer);
                        foreach (var root in gameObjectBuffer)
                        {
                            var others = root.GetComponentsInChildren<LifetimeScope>();
                            foreach (var other in others)
                            {
                                if (parentKey == other.Key)
                                {
                                    return parent;
                                }
                            }
                        }
                    }
                }
                throw new VContainerException($"LifetimeScope parent key `{parentKey}` is not found in any scene");
            }

            return ProjectRoot;
        }

        void DispatchPlayerLoopItems()
        {
            try
            {
                var initializables = Container.Resolve<IEnumerable<IInitializable>>();
                var loopItem = new InitializationLoopItem(initializables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Initialization, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var postInitializables = Container.Resolve<IEnumerable<IPostInitializable>>();
                var loopItem = new PostInitializationLoopItem(postInitializables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostInitialization, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var fixedTickables = Container.Resolve<IEnumerable<IFixedTickable>>();
                var loopItem = new FixedTickableLoopItem(fixedTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var postFixedTickables = Container.Resolve<IEnumerable<IPostFixedTickable>>();
                var loopItem = new PostFixedTickableLoopItem(postFixedTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var tickables = Container.Resolve<IEnumerable<ITickable>>();
                var loopItem = new TickableLoopItem(tickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var postTickables = Container.Resolve<IEnumerable<IPostTickable>>();
                var loopItem = new PostTickableLoopItem(postTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var lateTickables = Container.Resolve<IEnumerable<ILateTickable>>();
                var loopItem = new LateTickableLoopItem(lateTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }
            catch (VContainerException) { }

            try
            {
                var postLateTickables = Container.Resolve<IEnumerable<IPostLateTickable>>();
                var loopItem = new PostLateTickableLoopItem(postLateTickables);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }
            catch (VContainerException) { }
        }
    }
}
