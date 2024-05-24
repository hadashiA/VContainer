using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class CallbacksHolder : ScriptableSingleton<CallbacksHolder>, ICallbacksHolder
    {
        private List<CallbackWithPriority> m_Callbacks = new List<CallbackWithPriority>();
        public void Add(ICallbacks callback, int priority)
        {
            m_Callbacks.Add(new CallbackWithPriority(callback, priority));
        }

        public void Remove(ICallbacks callback)
        {
            m_Callbacks.RemoveAll(callbackWithPriority => callbackWithPriority.Callback == callback);
        }

        public ICallbacks[] GetAll()
        {
            return m_Callbacks.OrderByDescending(callback => callback.Priority).Select(callback => callback.Callback).ToArray();
        }

        public void Clear()
        {
            m_Callbacks.Clear();
        }

        private struct CallbackWithPriority
        {
            public ICallbacks Callback;
            public int Priority;
            public CallbackWithPriority(ICallbacks callback, int priority)
            {
                Callback = callback;
                Priority = priority;
            }
        }

        // Sometimes - such as when we want to test the test framework itself - it's necessary to launch a test run from
        // inside a test. Because callbacks are registered globally, this can cause a lot of confusion (e.g. the in-test
        // run will emit UTP messages, utterly confusing UTR). In such circumstances the safest thing to do is to
        // temporarily suppress all registered callbacks for the duration of the in-test run. This method can be called
        // to set up a using() block which will suppress the callbacks for the scope.
        public IDisposable TemporarilySuppressCallbacks()
        {
            return new Suppressor(this);
        }

        private sealed class Suppressor : IDisposable
        {
            private readonly CallbacksHolder _instance;
            private readonly List<CallbackWithPriority> _suppressed;

            public Suppressor(CallbacksHolder instance)
            {
                _instance = instance;
                _suppressed = new List<CallbackWithPriority>(instance.m_Callbacks);
                instance.m_Callbacks.Clear();
            }

            public void Dispose()
            {
                _instance.m_Callbacks.AddRange(_suppressed);
            }
        }
    }
}
