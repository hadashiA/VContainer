using System;

namespace VContainer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class KeyAttribute : PreserveAttribute
    {
        public object Key { get; }

        public KeyAttribute(object key = null)
        {
            Key = key;
        }
    }
}