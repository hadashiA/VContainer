using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// This attribute is an alternative to the standard `Ignore` attribute in [NUnit](https://nunit.org/). It allows for ignoring tests only under a specified condition. The condition evaluates during `OnLoad`, referenced by ID.
    /// </summary>
    public class ConditionalIgnoreAttribute : NUnitAttribute, IApplyToTest
    {
        private string m_ConditionKey;
        private string m_IgnoreReason;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalIgnoreAttribute"/> class with a condition key.
        /// </summary>
        /// <param name="conditionKey">The key to check for enabling the conditional ignore. The condition is set with the static <see cref="AddConditionalIgnoreMapping"/> method.</param>
        /// <param name="ignoreReason">The reason for the ignore.</param>
        public ConditionalIgnoreAttribute(string conditionKey, string ignoreReason)
        {
            m_ConditionKey = conditionKey;
            m_IgnoreReason = ignoreReason;
        }

        /// <summary>
        /// Modifies a test as defined for the specific attribute.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            var key = m_ConditionKey.ToLowerInvariant();
            if (m_ConditionMap.ContainsKey(key) && m_ConditionMap[key])
            {
                test.RunState = RunState.Ignored;
                string skipReason = string.Format(m_IgnoreReason);
                test.Properties.Add(PropertyNames.SkipReason, skipReason);
            }
        }

        private static Dictionary<string, bool> m_ConditionMap = new Dictionary<string, bool>();

        /// <summary>
        /// Adds a flag indicating whether tests with the same key should be ignored.
        /// </summary>
        /// <param name="key">The key to ignore tests for.</param>
        /// <param name="value">A boolean value indicating whether the tests should be ignored.</param>
        /// <example>
        /// An example in which tests are ignored in the Mac editor only.
        /// <code>
        /// using UnityEditor;
        /// using NUnit.Framework;
        /// using UnityEngine.TestTools;
        ///
        /// [InitializeOnLoad]
        /// public class OnLoad
        /// {
        ///     static OnLoad()
        ///     {
        ///         var editorIsOSX = false;
        ///         #if UNITY_EDITOR_OSX
        ///         editorIsOSX = true;
        ///         #endif
        ///
        ///         ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreInMacEditor", editorIsOSX);
        ///     }
        /// }
        ///
        /// public class MyTestClass
        /// {
        ///     [Test, ConditionalIgnore("IgnoreInMacEditor", "Ignored on Mac editor.")]
        ///     public void TestNeverRunningInMacEditor()
        ///     {
        ///         Assert.Pass();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void AddConditionalIgnoreMapping(string key, bool value)
        {
            m_ConditionMap.Add(key.ToLowerInvariant(), value);
        }
    }
}
