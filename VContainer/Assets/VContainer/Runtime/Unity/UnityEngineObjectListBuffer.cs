using System;
using System.Collections.Generic;

namespace VContainer.Unity
{
    static class UnityEngineObjectListBuffer<T> where T : UnityEngine.Object
    {
        const int DefaultCapacity = 32;

        [ThreadStatic]
        static List<T> Instance = new List<T>(DefaultCapacity);

        public static List<T> Get()
        {
            if (Instance == null)
                Instance = new List<T>(DefaultCapacity);
            Instance.Clear();
            return Instance;
        }
    }
}