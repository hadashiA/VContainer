using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEditor;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace UnityEngine.TestTools
{
    internal class StrictCheckCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        public StrictCheckCommand(TestCommand innerCommand) : base(innerCommand)
        {
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = UnityTestExecutionContext.CurrentContext;
            var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
            foreach (var iterator in executeEnumerable)
            {
                yield return iterator;
            }

            if (!unityContext.FeatureFlags.strictDomainReload)
            {
                yield break;
            }
            // Refreshing the asset database before the check, to ensure that
            // no potential pending domain reloads propagate to the following test
            // (due to ex. creation or deletion of asset files in the TearDown of a test).
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            if (isDomainReloadPending())
            {
                context.CurrentResult.SetResult(ResultState.Failure, "A pending domain reload was detected.");
            }
        }
        private static bool isDomainReloadPending()
        {
#if UNITY_EDITOR
            return (InternalEditorUtility.IsScriptReloadRequested() || EditorApplication.isCompiling);
#else
            return false;
#endif
        }
    }
}
