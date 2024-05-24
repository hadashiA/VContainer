using System;

namespace Reflex.Scripts.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class MonoInjectAttribute : Attribute { }
}