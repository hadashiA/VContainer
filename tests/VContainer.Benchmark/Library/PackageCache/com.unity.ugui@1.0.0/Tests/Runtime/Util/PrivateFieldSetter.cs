using System;
using System.Reflection;

namespace UnityEngine.UI.Tests
{
    class PrivateFieldSetter<T> : IDisposable
    {
        private object m_Obj;
        private FieldInfo m_FieldInfo;
        private object m_OldValue;

        public PrivateFieldSetter(object obj, string field, object value)
        {
            m_Obj = obj;
            m_FieldInfo = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            m_OldValue = m_FieldInfo.GetValue(obj);
            m_FieldInfo.SetValue(obj, value);
        }

        public void Dispose()
        {
            m_FieldInfo.SetValue(m_Obj, m_OldValue);
        }
    }

    static class PrivateStaticField
    {
        public static T GetValue<T>(Type staticType, string fieldName)
        {
            var type = staticType;
            FieldInfo field = null;
            while (field == null && type != null)
            {
                field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            return (T)field.GetValue(null);
        }
    }

    static class PrivateField
    {
        public static T GetValue<T>(this object o, string fieldName)
        {
            var type = o.GetType();
            FieldInfo field = null;
            while (field == null && type != null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            return field != null ? (T)field.GetValue(o) : default(T);
        }
    }
}
