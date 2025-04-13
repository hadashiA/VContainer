using System;

namespace VContainer
{
    public class PreserveAttribute : Attribute
    {
    }

#if UNITY_2018_4_OR_NEWER
    [JetBrains.Annotations.MeansImplicitUse(
        JetBrains.Annotations.ImplicitUseKindFlags.Access |
        JetBrains.Annotations.ImplicitUseKindFlags.Assign |
        JetBrains.Annotations.ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
#endif
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : PreserveAttribute
    {
        public object Id { get; }

        public InjectAttribute(object id = null)
        {
            Id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class InjectIgnoreAttribute : Attribute
    {
    }
}
