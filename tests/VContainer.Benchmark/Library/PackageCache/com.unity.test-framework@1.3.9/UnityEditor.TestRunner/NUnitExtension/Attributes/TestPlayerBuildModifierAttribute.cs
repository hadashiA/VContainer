using System;

namespace UnityEditor.TestTools
{
    /// <summary>
    /// The `TestPlayerBuildModifierAttribute` attribute can be applied to test assemblies (will affect every test in the assembly).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TestPlayerBuildModifierAttribute : Attribute
    {
        private Type m_Type;
        /// <summary>
        /// Initializes and returns an instance of TestPlayerBuildModifierAttribute or throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="type">A target type that implements ITestPlayerBuildModifier.</param>
        /// <exception cref="ArgumentException">Throws a <see cref="ArgumentException"/> if the type provided does not implemented the `ITestPlayerBuildModifier` interface. </exception>
        public TestPlayerBuildModifierAttribute(Type type)
        {
            var interfaceType = typeof(ITestPlayerBuildModifier);
            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format("Type provided to {0} does not implement {1}", GetType().Name, interfaceType.Name));
            }
            m_Type = type;
        }

        internal ITestPlayerBuildModifier ConstructModifier()
        {
            return Activator.CreateInstance(m_Type) as ITestPlayerBuildModifier;
        }
    }
}
