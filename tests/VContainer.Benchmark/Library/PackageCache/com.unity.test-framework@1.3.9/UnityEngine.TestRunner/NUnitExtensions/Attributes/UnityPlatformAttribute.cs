using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Use this attribute to define a specific set of platforms you want or do not want your test(s) to run on.
    ///
    /// You can use this attribute on the test method, test class, or test assembly level. Use the supported <see cref="RuntimePlatform"/> enumeration values to specify the platforms. You can also specify which platforms to test by passing one or more `RuntimePlatform` values along with or without the include or exclude properties as parameters to the [Platform](https://github.com/nunit/docs/wiki/Platform-Attribute) attribute constructor.
    ///
    /// The test(s) skips if the current target platform is:
    /// - Not explicitly specified in the included platforms list
    /// - In the excluded platforms list
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnityPlatformAttribute : NUnitAttribute, IApplyToTest
    {
        /// <summary>
        /// A subset of platforms you need to have your tests run on.
        /// </summary>
        public RuntimePlatform[] include { get; set; }
        /// <summary>
        /// List the platforms you do not want to have your tests run on.
        /// </summary>
        public RuntimePlatform[] exclude { get; set; }

        private string m_skippedReason;

        /// <summary>
        /// Constructs a new instance of the <see cref="UnityPlatformAttribute"/> class.
        /// </summary>
        public UnityPlatformAttribute()
        {
            include = new List<RuntimePlatform>().ToArray();
            exclude = new List<RuntimePlatform>().ToArray();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="UnityPlatformAttribute"/> class with a list of platforms to include.
        /// </summary>
        /// <param name="include">The different <see cref="RuntimePlatform"/> to run the test on.</param>
        public UnityPlatformAttribute(params RuntimePlatform[] include)
            : this()
        {
            this.include = include;
        }

        /// <summary>
        /// Modifies a test as defined for the specific attribute.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable || test.RunState == RunState.Ignored || IsPlatformSupported(Application.platform))
            {
                return;
            }
            test.RunState = RunState.Skipped;
            test.Properties.Add(PropertyNames.SkipReason, m_skippedReason);
        }

        internal bool IsPlatformSupported(RuntimePlatform testTargetPlatform)
        {
            if (include.Any() && !include.Any(x => x == testTargetPlatform))
            {
                m_skippedReason = string.Format("Only supported on {0}", string.Join(", ", include.Select(x => x.ToString()).ToArray()));
                return false;
            }

            if (exclude.Any(x => x == testTargetPlatform))
            {
                m_skippedReason = string.Format("Not supported on  {0}", string.Join(", ", exclude.Select(x => x.ToString()).ToArray()));
                return false;
            }
            return true;
        }
    }
}
