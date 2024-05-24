using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestTools;
using SetUpTearDownCommand = UnityEngine.TestTools.SetUpTearDownCommand;
using TestActionCommand = UnityEngine.TestTools.TestActionCommand;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal static class TestCommandBuilder
    {
        public static TestCommand BuildTestCommand(TestMethod test, ITestFilter filter)
        {
            if (test.RunState != RunState.Runnable &&
                !(test.RunState == RunState.Explicit && filter.IsExplicitMatch(test)))
            {
                return new SkipCommand(test);
            }

            var testReturnsIEnumerator = test.Method.ReturnType.Type == typeof(IEnumerator);
            var testReturnsTask = test.Method.ReturnType.Type == typeof(Task);

            TestCommand command;
            if (testReturnsTask)
            {
                command = new TaskTestMethodCommand(test);
            }
            else if (testReturnsIEnumerator)
            {
                command = new EnumerableTestMethodCommand(test);
            }
            else
            {
                command = new UnityTestMethodCommand(test);
            }

            command = new UnityLogCheckDelegatingCommand(command);
            foreach (var wrapper in test.Method.GetCustomAttributes<IWrapTestMethod>(true))
            {
                command = wrapper.Wrap(command);
                if (command == null)
                {
                    var message = String.Format("IWrapTestMethod implementation '{0}' returned null as command.",
                        wrapper.GetType().FullName);
                    return new FailCommand(test, ResultState.Failure, message);
                }

                if (testReturnsIEnumerator && !(command is IEnumerableTestMethodCommand))
                {
                    command = TryReplaceWithEnumerableCommand(command);
                    if (command != null)
                    {
                        continue;
                    }

                    var message = String.Format("'{0}' is not supported on {1} as it does not handle returning IEnumerator.",
                        wrapper.GetType().FullName,
                        GetTestBuilderName(test));
                    return new FailCommand(test, ResultState.Failure, message);
                }
            }

            command = new TestActionCommand(command);

            if (!testReturnsIEnumerator && !testReturnsTask)
            {
                command = new ImmediateEnumerableCommand(command);
            }
            
            command = new SetUpTearDownCommand(command);
            
            foreach (var wrapper in test.Method.GetCustomAttributes<IWrapSetUpTearDown>(true))
            {
                if (command is SetUpTearDownCommand && !testReturnsIEnumerator && !testReturnsTask)
                {
                    // Ensure that we can use the immediate execute on the setup/teardown
                    command = new ImmediateEnumerableCommand(command);
                }

                command = wrapper.Wrap(command);
                if (command == null)
                {
                    var message = String.Format("IWrapSetUpTearDown implementation '{0}' returned null as command.",
                        wrapper.GetType().FullName);
                    return new FailCommand(test, ResultState.Failure, message);
                }

                if (testReturnsIEnumerator && !(command is IEnumerableTestMethodCommand))
                {
                    command = TryReplaceWithEnumerableCommand(command);
                    if (command != null)
                    {
                        continue;
                    }

                    var message = String.Format("'{0}' is not supported on {1} as it does not handle returning IEnumerator.",
                        wrapper.GetType().FullName,
                        GetTestBuilderName(test));
                    return new FailCommand(test, ResultState.Failure, message);
                }
            }

            command = new EnumerableSetUpTearDownCommand(command);
            command = new OuterUnityTestActionCommand(command);
            command = new RetryCommand(command);
            command = new RepeatCommand(command);
            
            IApplyToContext[] changes = test.Method.GetCustomAttributes<IApplyToContext>(true);
            if (changes.Length > 0)
            {
                command = new EnumerableApplyChangesToContextCommand(command, changes);
            }

            command = new TimeoutCommand(command);
            command = new IgnoreTestCommand(command, test);
            command = new StrictCheckCommand(command);
            return command;
        }

        private static string GetTestBuilderName(TestMethod testMethod)
        {
            return new[]
            {
                testMethod.Method.GetCustomAttributes<ITestBuilder>(true).Select(attribute => attribute.GetType().Name),
                testMethod.Method.GetCustomAttributes<ISimpleTestBuilder>(true).Select(attribute => attribute.GetType().Name)
            }.SelectMany(v => v).FirstOrDefault();
        }

        private static TestCommand TryReplaceWithEnumerableCommand(TestCommand command)
        {
            switch (command.GetType().Name)
            {
                case nameof(RepeatAttribute.RepeatedTestCommand):
                    return new EnumerableRepeatedTestCommand(command as RepeatAttribute.RepeatedTestCommand);
                case nameof(RetryAttribute.RetryCommand):
                    return new EnumerableRetryTestCommand(command as RetryAttribute.RetryCommand);
                default:
                    return null;
            }
        }
    }
}
