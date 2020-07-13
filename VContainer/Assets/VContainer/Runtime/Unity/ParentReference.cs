using System;
using UnityEngine;

namespace VContainer.Unity
{
    [Serializable]
    public struct ParentReference : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string TypeName;

        [NonSerialized]
        public LifetimeScope Object;

        public Type Type { get; private set; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            TypeName = Type?.FullName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(TypeName))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type = assembly.GetType(TypeName);
                    if (Type != null)
                        break;
                }
            }
        }
    }
}