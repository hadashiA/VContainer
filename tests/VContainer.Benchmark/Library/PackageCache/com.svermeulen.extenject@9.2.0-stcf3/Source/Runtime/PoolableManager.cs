using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class PoolableManager
    {
        readonly List<IPoolable> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned()
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned();
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable Poolable;
            public int Priority;

            public PoolableInfo(IPoolable poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }

    /// <summary>
    /// A modified version of PoolableManager that adds a generic argument, allowing
    /// the passing of a parameter to all IPoolable<T> objects in the container.
    /// </summary>
    public class PoolableManager<T>
    {
        readonly List<IPoolable<T>> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable<T>> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable<T> poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned(T param)
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned(param);
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable<T> Poolable;
            public int Priority;

            public PoolableInfo(IPoolable<T> poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }

    /// <summary>
    /// A modified version of PoolableManager that adds a generic argument, allowing
    /// the passing of a parameter to all IPoolable<T1, T2> objects in the container.
    /// </summary>
    public class PoolableManager<T1, T2>
    {
        readonly List<IPoolable<T1, T2>> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable<T1, T2>> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable<T1, T2> poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned(T1 p1, T2 p2)
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned(p1, p2);
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable<T1, T2> Poolable;
            public int Priority;

            public PoolableInfo(IPoolable<T1, T2> poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }

    /// <summary>
    /// A modified version of PoolableManager that adds a generic argument, allowing
    /// the passing of a parameter to all IPoolable<T1, T2> objects in the container.
    /// </summary>
    public class PoolableManager<T1, T2, T3>
    {
        readonly List<IPoolable<T1, T2, T3>> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable<T1, T2, T3>> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable<T1, T2, T3> poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned(T1 p1, T2 p2, T3 p3)
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned(p1, p2, p3);
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable<T1, T2, T3> Poolable;
            public int Priority;

            public PoolableInfo(IPoolable<T1, T2, T3> poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }

    /// <summary>
    /// A modified version of PoolableManager that adds a generic argument, allowing
    /// the passing of a parameter to all IPoolable<T1, T2> objects in the container.
    /// </summary>
    public class PoolableManager<T1, T2, T3, T4>
    {
        readonly List<IPoolable<T1, T2, T3, T4>> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable<T1, T2, T3, T4>> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable<T1, T2, T3, T4> poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned(p1, p2, p3, p4);
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable<T1, T2, T3, T4> Poolable;
            public int Priority;

            public PoolableInfo(IPoolable<T1, T2, T3, T4> poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }


    /// <summary>
    /// A modified version of PoolableManager that adds a generic argument, allowing
    /// the passing of a parameter to all IPoolable<T1, T2> objects in the container.
    /// </summary>
    public class PoolableManager<T1, T2, T3, T4, T5>
    {
        readonly List<IPoolable<T1, T2, T3, T4, T5>> _poolables;

        bool _isSpawned;

        public PoolableManager(
            [InjectLocal]
            List<IPoolable<T1, T2, T3, T4, T5>> poolables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities)
        {
            _poolables = poolables.Select(x => CreatePoolableInfo(x, priorities))
                .OrderBy(x => x.Priority).Select(x => x.Poolable).ToList();
        }

        PoolableInfo CreatePoolableInfo(IPoolable<T1, T2, T3, T4, T5> poolable, List<ValuePair<Type, int>> priorities)
        {
            var match = priorities.Where(x => poolable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)(x.Second)).SingleOrDefault();
            int priority = match.HasValue ? match.Value : 0;

            return new PoolableInfo(poolable, priority);
        }

        public void TriggerOnSpawned(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            Assert.That(!_isSpawned);
            _isSpawned = true;

            for (int i = 0; i < _poolables.Count; i++)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnSpawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnSpawned(p1, p2, p3, p4, p5);
                }
            }
        }

        public void TriggerOnDespawned()
        {
            Assert.That(_isSpawned);
            _isSpawned = false;

            // Call OnDespawned in the reverse order just like how dispose works
            for (int i = _poolables.Count - 1; i >= 0; i--)
            {
#if ZEN_INTERNAL_PROFILING
                using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                using (ProfileBlock.Start("{0}.OnDespawned", _poolables[i].GetType()))
#endif
                {
                    _poolables[i].OnDespawned();
                }
            }
        }

        struct PoolableInfo
        {
            public IPoolable<T1, T2, T3, T4, T5> Poolable;
            public int Priority;

            public PoolableInfo(IPoolable<T1, T2, T3, T4, T5> poolable, int priority)
            {
                Poolable = poolable;
                Priority = priority;
            }
        }
    }
}
