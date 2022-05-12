using System;

namespace VContainer
{
    public class PreserveAttribute : Attribute
    {
    }

#if UNITY_2018_4_OR_NEWER
    [JetBrains.Annotations.MeansImplicitUse(JetBrains.Annotations.ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
#endif
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : PreserveAttribute
    {
    }
}