using UnityEngine;

namespace VContainer.Unity.Extensions
{
    public abstract class SingletonLifetimeScope<T> : LifetimeScope where T : SingletonLifetimeScope<T>
    {
        static T s_instance;
        
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObject();
                    if (s_instance != null)
                    {
                        s_instance.Initialize();
                    }
                    else
                    {
                        Debug.LogError($"The instance of {typeof(T).Name} was not found. Please make sure it is present in the scene.");
                    }
                }
                return s_instance;
            }
        }

        void Initialize()
        {
            base.Awake();
            s_instance = (T) this;
            DontDestroyOnLoad(gameObject);
        }
        
        protected override void Awake()
        {
            if (s_instance == null)
            {
                Initialize();
            }
            else if (s_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        protected override void OnDestroy()
        {
            if (s_instance == this)
            {
                base.OnDestroy();
                s_instance = null;
            }
        }

        protected void OnValidate()
        {
            if (parentReference.Type != null)
            {
                Reset();
            }
        }

        protected virtual void Reset()
        {
            parentReference = default;
        }
        
        static T FindObject()
        {
#if UNITY_2022_1_OR_NEWER
            return FindAnyObjectByType<T>();
#else
            return FindObjectOfType<T>();
#endif
        }
    }
}