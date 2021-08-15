using System;
using System.Collections.Generic;
using VContainer.Internal;
#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;
#endif

namespace VContainer.Unity
{
    public sealed class EntryPointDispatcher : IDisposable
    {
        readonly IObjectResolver container;
        readonly CompositeDisposable disposable = new CompositeDisposable();

        [Inject]
        public EntryPointDispatcher(IObjectResolver container)
        {
            this.container = container;
        }

        public void Dispatch()
        {
            PlayerLoopHelper.Initialize();

            EntryPointExceptionHandler exceptionHandler = null;
            try
            {
                exceptionHandler = container.Resolve<EntryPointExceptionHandler>();
            }
            catch (VContainerException ex) when (ex.InvalidType == typeof(EntryPointExceptionHandler))
            {
            }

            var initializables = container.Resolve<IReadOnlyList<IInitializable>>();
            for (var i = 0; i < initializables.Count; i++)
            {
                try
                {
                    initializables[i].Initialize();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        UnityEngine.Debug.LogException(ex);
                }
            }

            var postInitializables = container.Resolve<IReadOnlyList<IPostInitializable>>();
            for (var i = 0; i < postInitializables.Count; i++)
            {
                try
                {
                    postInitializables[i].PostInitialize();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        UnityEngine.Debug.LogException(ex);
                }
            }

            var startables = container.Resolve<IReadOnlyList<IStartable>>();
            if (startables.Count > 0)
            {
                var loopItem = new StartableLoopItem(startables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }

            var postStartables = container.Resolve<IReadOnlyList<IPostStartable>>();
            if (postStartables.Count > 0)
            {
                var loopItem = new PostStartableLoopItem(postStartables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostStartup, loopItem);
            }

            var fixedTickables = container.Resolve<IReadOnlyList<IFixedTickable>>();
            if (fixedTickables.Count > 0)
            {
                var loopItem = new FixedTickableLoopItem(fixedTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }

            var postFixedTickables = container.Resolve<IReadOnlyList<IPostFixedTickable>>();
            if (postFixedTickables.Count > 0)
            {
                var loopItem = new PostFixedTickableLoopItem(postFixedTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }

            var tickables = container.Resolve<IReadOnlyList<ITickable>>();
            if (tickables.Count > 0)
            {
                var loopItem = new TickableLoopItem(tickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }

            var postTickables = container.Resolve<IReadOnlyList<IPostTickable>>();
            if (postTickables.Count > 0)
            {
                var loopItem = new PostTickableLoopItem(postTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }

            var lateTickables = container.Resolve<IReadOnlyList<ILateTickable>>();
            if (lateTickables.Count > 0)
            {
                var loopItem = new LateTickableLoopItem(lateTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }

            var postLateTickables = container.Resolve<IReadOnlyList<IPostLateTickable>>();
            if (postLateTickables.Count > 0)
            {
                var loopItem = new PostLateTickableLoopItem(postLateTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }

#if VCONTAINER_UNITASK_INTEGRATION
            var asyncStartables = container.Resolve<IReadOnlyList<IAsyncStartable>>();
            if (asyncStartables.Count > 0)
            {
                var loopItem = new AsyncStartableLoopItem(asyncStartables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }
#endif

#if VCONTAINER_ECS_INTEGRATION
            container.Resolve<IEnumerable<ComponentSystemBase>>();

            var worldHelpers = container.Resolve<IEnumerable<WorldConfigurationHelper>>();
            foreach (var x in worldHelpers)
            {
                x.SortSystems();
            }
#endif
        }

        public void Dispose() => disposable.Dispose();
    }
}