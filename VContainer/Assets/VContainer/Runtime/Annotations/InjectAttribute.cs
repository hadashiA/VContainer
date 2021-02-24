using System;

namespace VContainer
{
    public class PreserveAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : PreserveAttribute
    {
    }
}