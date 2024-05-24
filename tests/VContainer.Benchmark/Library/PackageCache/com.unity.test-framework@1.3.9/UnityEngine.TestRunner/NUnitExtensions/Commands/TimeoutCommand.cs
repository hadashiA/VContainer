using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal class TimeoutCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        internal const int k_DefaultTimeout = 1000 * 180;

        public TimeoutCommand(TestCommand innerCommand) : base(innerCommand)
        {
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            if (context.TestCaseTimeout == 0)
            {
                context.TestCaseTimeout = k_DefaultTimeout;
            }
            
            var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
            foreach (var iterator in executeEnumerable)
            {
                var ticksDelta = Stopwatch.GetTimestamp() - context.StartTicks;
                if (ticksDelta > context.TestCaseTimeout  * (Stopwatch.Frequency / 1000f))
                {
                    context.CurrentResult.SetResult(ResultState.Error, new UnityTestTimeoutException(context.TestCaseTimeout).Message);
                    yield return new RestoreTestContextAfterDomainReload(); // If this is right after a domain reload, give the editor a chance to restore.
                    yield break;
                }
                yield return iterator;
            }
        }
    }
}