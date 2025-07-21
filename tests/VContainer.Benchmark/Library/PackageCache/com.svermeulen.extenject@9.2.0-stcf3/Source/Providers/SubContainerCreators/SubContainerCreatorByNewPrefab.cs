#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    [NoReflectionBaking]
    public class SubContainerCreatorByNewPrefab : ISubContainerCreator
    {
        readonly GameObjectCreationParameters _gameObjectBindInfo;
        readonly IPrefabProvider _prefabProvider;
        readonly DiContainer _container;

        public SubContainerCreatorByNewPrefab(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefabProvider = prefabProvider;
            _container = container;
        }

        public DiContainer CreateSubContainer(
            List<TypeValuePair> args, InjectContext parentContext, out Action injectAction)
        {
            Assert.That(args.IsEmpty());

            var prefab = _prefabProvider.GetPrefab(parentContext);

            bool shouldMakeActive;
            var gameObject = _container.CreateAndParentPrefab(
                prefab, _gameObjectBindInfo, null, out shouldMakeActive);

            var context = gameObject.GetComponent<GameObjectContext>();

            Assert.That(context != null,
                "Expected prefab with name '{0}' to contain a component of type 'GameObjectContext' on the root", prefab.name);

            context.Install(_container);

            injectAction = () =>
            {
                // Note: We don't need to call ResolveRoots here because GameObjectContext does this for us
                _container.Inject(context);

                if (shouldMakeActive && !_container.IsValidating)
                {
#if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
                    {
                        gameObject.SetActive(true);
                    }
                }
            };

            return context.Container;
        }
    }
}

#endif
