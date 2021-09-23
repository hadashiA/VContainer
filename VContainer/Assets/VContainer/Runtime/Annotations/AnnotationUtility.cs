using System;
using System.Collections.Generic;

namespace VContainer.Unity
{
    public static class AnnotationUtility
    {
        private static readonly List<Type> _list = new List<Type>
        {
#if VCONTAINER_UNITASK_INTEGRATION
            typeof(IAsyncStartable),
#endif
            typeof(IInitializable),
            typeof(IPostInitializable),
            typeof(IStartable),
            typeof(IPostStartable),
            typeof(IFixedTickable),
            typeof(IPostFixedTickable),
            typeof(ITickable),
            typeof(IPostTickable),
            typeof(ILateTickable),
            typeof(IPostLateTickable)
        };

        public static bool IsLifecycleAnnotation(Type type) => _list.Contains(type);
    }
}