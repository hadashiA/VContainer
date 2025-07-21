using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [NoReflectionBaking]
    public class CachedOpenTypeProvider : IProvider
    {
        readonly IProvider _creator;
        readonly List<List<object>> _cachedInstances = new List<List<object>>();

#if ZEN_MULTITHREADING
        readonly object _locker = new object();
#else
        bool _isCreatingInstance;
#endif

        public CachedOpenTypeProvider(IProvider creator)
        {
            Assert.That(creator.TypeVariesBasedOnMemberType);
            _creator = creator;
        }

        public bool IsCached
        {
            get { return true; }
        }

        public bool TypeVariesBasedOnMemberType
        {
            get
            {
                // Should not call this
                throw Assert.CreateException();
            }
        }

        public int NumInstances
        {
            get
            {
#if ZEN_MULTITHREADING
                lock (_locker)
#endif
                {
                    return _cachedInstances.Select(x => x.Count).Sum();
                }
            }
        }

        // This method can be called if you want to clear the memory for an AsSingle instance,
        // See isssue https://github.com/svermeulen/Zenject/issues/441
        public void ClearCache()
        {
#if ZEN_MULTITHREADING
            lock (_locker)
#endif
            {
                _cachedInstances.Clear();
            }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _creator.GetInstanceType(context);
        }

        List<object> TryGetMatchFromCache(Type memberType)
        {
            List<object> result = null;

            for (int i = 0; i < _cachedInstances.Count; i++) 
            {
                var instanceList = _cachedInstances[i];

                bool matchesAll = true;

                for (int k = 0; k < instanceList.Count; k++) 
                {
                    var instance = instanceList[k];

                    if (instance == null) 
                    {
                        if (memberType.IsValueType()) 
                        {
                            matchesAll = false;
                            break;
                        }

                        continue;
                    }

                    if (!instance.GetType().DerivesFromOrEqual(memberType)) 
                    {
                        matchesAll = false;
                        break;
                    }
                }

                if (matchesAll) 
                {
                    Assert.IsNull(result); // Is there any case where this is hit?
                    result = instanceList;
                }
            }

            return result;
        }

        public void GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction, List<object> buffer)
        {
            Assert.IsNotNull(context);

#if ZEN_MULTITHREADING
            lock (_locker)
#endif
            {
                var instances = TryGetMatchFromCache(context.MemberType);

                if (instances != null)
                {
                    injectAction = null;
                    buffer.AllocFreeAddRange(instances);
                    return;
                }

#if !ZEN_MULTITHREADING
                // This should only happen with constructor injection
                // Field or property injection should allow circular dependencies
                if (_isCreatingInstance)
                {
                    var instanceType = _creator.GetInstanceType(context);
                    throw Assert.CreateException(
                        "Found circular dependency when creating type '{0}'. Object graph:\n {1}{2}\n",
                        instanceType, context.GetObjectGraphString(), instanceType);
                }

                _isCreatingInstance = true;
#endif

                instances = new List<object>();
                _creator.GetAllInstancesWithInjectSplit(
                    context, args, out injectAction, instances);
                Assert.IsNotNull(instances);

                _cachedInstances.Add(instances);
#if !ZEN_MULTITHREADING
                _isCreatingInstance = false;
#endif
                buffer.AllocFreeAddRange(instances);
            }
        }
    }
}

