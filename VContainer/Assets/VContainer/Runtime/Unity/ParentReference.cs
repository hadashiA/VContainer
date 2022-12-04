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

        public ParentReference(Type type)
        {
            if (!typeof(LifetimeScope).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type} does not inherit {nameof(LifetimeScope)}. Be sure to inherit from it.");
            }
            Type = type;
            TypeName = type.FullName;
            Object = null;
        }

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

        public static ParentReference Create<T>() where T : LifetimeScope
        {
            return new ParentReference(typeof(T));
        }
    }
}