using System.Collections.Generic;
#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;
#endif

namespace VContainer.Unity
{
    partial class LifetimeScope
    {
        void DispatchEntryPoints()
        {
            PlayerLoopHelper.Initialize();

            EntryPointExceptionHandler exceptionHandler = null;
            try
            {
                exceptionHandler = Container.Resolve<EntryPointExceptionHandler>();
            }
            catch (VContainerException ex) when (ex.InvalidType == typeof(EntryPointExceptionHandler))
            {
            }

            var initializables = Container.Resolve<IReadOnlyList<IInitializable>>();
            if (initializables.Count > 0)
            {
                var loopItem = new InitializationLoopItem(initializables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Initialization, loopItem);
            }

            var postInitializables = Container.Resolve<IReadOnlyList<IPostInitializable>>();
            if (postInitializables.Count > 0)
            {
                var loopItem = new PostInitializationLoopItem(postInitializables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostInitialization, loopItem);
            }

            var startables = Container.Resolve<IReadOnlyList<IStartable>>();
            if (startables.Count > 0)
            {
                var loopItem = new StartableLoopItem(startables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }

            var postStartables = Container.Resolve<IReadOnlyList<IPostStartable>>();
            if (postStartables.Count > 0)
            {
                var loopItem = new PostStartableLoopItem(postStartables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostStartup, loopItem);
            }

            var fixedTickables = Container.Resolve<IReadOnlyList<IFixedTickable>>();
            if (fixedTickables.Count > 0)
            {
                var loopItem = new FixedTickableLoopItem(fixedTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }

            var postFixedTickables = Container.Resolve<IReadOnlyList<IPostFixedTickable>>();
            if (postFixedTickables.Count > 0)
            {
                var loopItem = new PostFixedTickableLoopItem(postFixedTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }

            var tickables = Container.Resolve<IReadOnlyList<ITickable>>();
            if (tickables.Count > 0)
            {
                var loopItem = new TickableLoopItem(tickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }

            var postTickables = Container.Resolve<IReadOnlyList<IPostTickable>>();
            if (postTickables.Count > 0)
            {
                var loopItem = new PostTickableLoopItem(postTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }

            var lateTickables = Container.Resolve<IReadOnlyList<ILateTickable>>();
            if (lateTickables.Count > 0)
            {
                var loopItem = new LateTickableLoopItem(lateTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }

            var postLateTickables = Container.Resolve<IReadOnlyList<IPostLateTickable>>();
            if (postLateTickables.Count > 0)
            {
                var loopItem = new PostLateTickableLoopItem(postLateTickables, exceptionHandler);
                disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }

#if VCONTAINER_UNITASK_INTEGRATION
            var asyncStartables = Container.Resolve<IReadOnlyList<IAsyncStartable>>();
            if (asyncStartables.Count > 0)
            {
                var loopItem = new AsyncStartableLoopItem(asyncStartables, exceptionHandler);
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
