using System.Collections.Generic;
using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class VContainerLifetimeScope : MonoBehaviour
    {
        [SerializeField]
        VContainerInstaller[] installers;

        [SerializeField]
        VContainerLifetimeScope parent;

        public IObjectResolver Container { get; private set; }
        readonly CompositeDisposable disposable = new CompositeDisposable();

        void Awake()
        {
            if (parent is VContainerLifetimeScope parentScope)
            {
                Container = parentScope.Container.CreateScope(builder =>
                {
                    var decoratedBuilder = new ContainerBuilderUnity(builder, this);
                    foreach (var installer in installers)
                    {
                        installer.Install(decoratedBuilder);
                    }
                });
            }
            else
            {
                var builder = new VContainer.ContainerBuilder();
                var decoratedBuilder = new ContainerBuilderUnity(builder, this);
                foreach (var installer in installers)
                {
                    installer.Install(decoratedBuilder);
                }
                Container = builder.Build();
            }

            DispatchPlayerLoopItems();
        }

        void OnDestroy()
        {
            Container.Dispose();
            disposable.Dispose();
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
