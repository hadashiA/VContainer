using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class IgnoreTestCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private ITest _test;
        public IgnoreTestCommand(TestCommand innerCommand, ITest test) : base(innerCommand)
        {
            _test = test;
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var ignoreTests = ((UnityTestExecutionContext) context).IgnoreTests;
            if (ignoreTests != null && ignoreTests.Length > 0)
            {
                var fullName = _test.GetFullNameWithoutDllPath();
                foreach (var ignoreTest in ignoreTests)
                {
                    if (ignoreTest.test.Equals(fullName))
                    {
                        context.CurrentResult.SetResult(ResultState.Ignored,ignoreTest.ignoreComment);
                        yield break;
                    }
                }
            }
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
        }
    }
}
