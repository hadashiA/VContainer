using System;

namespace VContainer
{
    public class PreserveAttribute : Attribute
    {
    }

    /// <summary>
    /// Any method, property, attribute, or constructor annotated with this attribute
    /// will be used to inject dependencies.
    /// </summary>
    /// <remarks>
    /// It is an error to apply this attribute to more than one constructor. However,
    /// it can be applied to any number of methods, properties, or fields.
    /// </remarks>
    /// <seealso cref="IObjectResolver.Inject"/>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : PreserveAttribute
    {
    }
}