using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEditor.TestTools
{
    /// <summary>
    /// Ignore attributes dedicated to Asset Import Pipeline backend version handling.
    /// </summary>
    internal static class AssetPipelineIgnore
    {
        internal enum AssetPipelineBackend
        {
            V1,
            V2
        }

        /// <summary>
        /// Ignore the test when running with the legacy Asset Import Pipeline V1 backend.
        /// </summary>
        internal class IgnoreInV1 : AssetPipelineIgnoreAttribute
        {
            public IgnoreInV1(string ignoreReason) : base(AssetPipelineBackend.V1, ignoreReason) {}
        }

        /// <summary>
        /// Ignore the test when running with the latest Asset Import Pipeline V2 backend.
        /// </summary>
        internal class IgnoreInV2 : AssetPipelineIgnoreAttribute
        {
            public IgnoreInV2(string ignoreReason) : base(AssetPipelineBackend.V2, ignoreReason) {}
        }

        [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
        internal class AssetPipelineIgnoreAttribute : NUnitAttribute, IApplyToTest
        {
            private readonly string m_IgnoreReason;
            private readonly AssetPipelineBackend m_IgnoredBackend;
            private static readonly AssetPipelineBackend k_ActiveBackend = AssetDatabase.IsV2Enabled()
                ? AssetPipelineBackend.V2
                : AssetPipelineBackend.V1;

            private static string ActiveBackendName = Enum.GetName(typeof(AssetPipelineBackend), k_ActiveBackend);

            public AssetPipelineIgnoreAttribute(AssetPipelineBackend backend, string ignoreReason)
            {
                m_IgnoredBackend = backend;
                m_IgnoreReason = ignoreReason;
            }

            public void ApplyToTest(Test test)
            {
                if (k_ActiveBackend == m_IgnoredBackend)
                {
                    test.RunState = RunState.Ignored;
                    var skipReason = string.Format("Not supported by asset pipeline {0} backend {1}", ActiveBackendName, m_IgnoreReason);
                    test.Properties.Add(PropertyNames.SkipReason, skipReason);
                }
            }
        }
    }
}
