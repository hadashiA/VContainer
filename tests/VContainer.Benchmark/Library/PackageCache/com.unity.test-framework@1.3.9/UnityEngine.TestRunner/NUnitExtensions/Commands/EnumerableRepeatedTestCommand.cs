using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class EnumerableRepeatedTestCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private int repeatCount;

        public EnumerableRepeatedTestCommand(RepeatAttribute.RepeatedTestCommand commandToReplace) : base(commandToReplace.GetInnerCommand())
        {
            repeatCount = (int)typeof(RepeatAttribute.RepeatedTestCommand)
                .GetField("repeatCount", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(commandToReplace);
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = (UnityTestExecutionContext)context;
            int count = unityContext.EnumerableTestState.Repeat;
            var firstCycleAfterResume = count > 0;

            while (count < repeatCount || (firstCycleAfterResume && count <= repeatCount))
            {
                if (!firstCycleAfterResume)
                {
                    count++;
                }

                firstCycleAfterResume = false;
                unityContext.EnumerableTestState.Repeat = count;

                if (innerCommand is IEnumerableTestMethodCommand)
                {
                    var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
                    foreach (var iterator in executeEnumerable)
                    {
                        yield return iterator;
                    }
                }
                else
                {
                    context.CurrentResult = innerCommand.Execute(context);
                }

                if (context.CurrentResult.ResultState != ResultState.Success)
                {
                    break;
                }
            }

            unityContext.EnumerableTestState.Repeat = 0;
        }
    }
}
