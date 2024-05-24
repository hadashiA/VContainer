using System;

namespace UnityEngine.TestTools.Constraints
{
    /// <summary>
    /// Extension of the `Is` class in NUnit.Framework, see [Is](https://docs.microsoft.com/en-us/dotnet/api/nunit.framework.is?view=xamarin-ios-sdk-12). 
    /// </summary>
    public class Is : NUnit.Framework.Is
    {
        /// <summary>
        /// Creates a new instance of `AllocatingGCMemoryConstraint`.
        /// </summary>
        /// <returns>A new AllocatingGCMemoryConstraint object.</returns>
        public static AllocatingGCMemoryConstraint AllocatingGCMemory()
        {
            return new AllocatingGCMemoryConstraint();
        }
    }
}
