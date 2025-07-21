using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class UnityLogCheckDelegatingCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private static Dictionary<object, bool?> s_AttributeCache = new Dictionary<object, bool?>();

        public UnityLogCheckDelegatingCommand(TestCommand innerCommand)
            : base(innerCommand) {}

        public override TestResult Execute(ITestExecutionContext context)
        {
            using (var logScope = new LogScope())
            {
                if (ExecuteAndCheckLog(logScope, context.CurrentResult, () => innerCommand.Execute(context)))
                    PostTestValidation(logScope, innerCommand, context.CurrentResult);
            }

            return context.CurrentResult;
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            if (!(innerCommand is IEnumerableTestMethodCommand enumerableTestMethodCommand))
            {
                Execute(context);
                yield break;
            }

            using (var logScope = new LogScope())
            {
                IEnumerable executeEnumerable = null;

                if (!ExecuteAndCheckLog(logScope, context.CurrentResult,
                    () => executeEnumerable = enumerableTestMethodCommand.ExecuteEnumerable(context)))
                    yield break;

                foreach (var step in executeEnumerable)
                {
                    // do not check expected logs here - we want to permit expecting and receiving messages to run
                    // across frames. (but we do always want to catch a fail immediately.)
                    if (!CheckFailingLogs(logScope, context.CurrentResult))
                        yield break;

                    yield return step;
                }

                if (!CheckLogs(context.CurrentResult, logScope))
                    yield break;

                PostTestValidation(logScope, innerCommand, context.CurrentResult);
            }
        }

        private static bool CaptureException(TestResult result, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                result.RecordException(e);
                return false;
            }
        }

        private static bool ExecuteAndCheckLog(LogScope logScope, TestResult result, Action action)
            => CaptureException(result, action) && CheckLogs(result, logScope);

        private static void PostTestValidation(LogScope logScope, TestCommand command, TestResult result)
        {
            if (MustExpect(command.Test.Method.MethodInfo))
                CaptureException(result, logScope.NoUnexpectedReceived);
        }

        private static bool CheckLogs(TestResult result, LogScope logScope)
        {
            try
            {
                logScope.EvaluateLogScope(true);
            }
            catch (Exception e)
            {
                result.RecordException(e);
                return false;
            }

            return true;
        }

        private static bool CheckFailingLogs(LogScope logScope, TestResult result)
        {
            try
            {
                logScope.EvaluateLogScope(false);
            }
            catch (Exception e)
            {
                result.RecordException(e);
                return false;
            }

            return true;
        }

        private static bool MustExpect(MemberInfo method)
        {
            // method

            var methodAttr = method.GetCustomAttributes<TestMustExpectAllLogsAttribute>(true).FirstOrDefault();
            if (methodAttr != null)
                return methodAttr.MustExpect;

            // fixture

            var fixture = method.DeclaringType;
            if (!s_AttributeCache.TryGetValue(fixture, out var mustExpect))
            {
                var fixtureAttr = fixture.GetCustomAttributes<TestMustExpectAllLogsAttribute>(true).FirstOrDefault();
                mustExpect = s_AttributeCache[fixture] = fixtureAttr?.MustExpect;
            }

            if (mustExpect != null)
                return mustExpect.Value;

            // assembly

            var assembly = fixture.Assembly;
            if (!s_AttributeCache.TryGetValue(assembly, out mustExpect))
            {
                var assemblyAttr = assembly.GetCustomAttributes<TestMustExpectAllLogsAttribute>().FirstOrDefault();
                mustExpect = s_AttributeCache[assembly] = assemblyAttr?.MustExpect;
            }

            return mustExpect == true;
        }
    }
}
