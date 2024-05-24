using System;
using System.Reflection;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal sealed class TestTreeViewItem : TreeViewItem
    {
        public TestRunnerResult result;
        internal ITestAdaptor m_Test;

        public Type type;
        public MethodInfo method;

        private const int k_ResultTestMaxLength = 15000;

        public bool IsGroupNode { get { return m_Test.IsSuite; } }

        public string FullName { get { return m_Test.FullName; } }
        
        public string GetAssemblyName()
        {
            var test = m_Test;
            while (test != null)
            {
                if (test.IsTestAssembly)
                {
                    return test.FullName;
                }

                test = test.Parent;
            }

            return null;
        }

        public TestTreeViewItem(ITestAdaptor test, int depth, TreeViewItem parent)
            : base(GetId(test), depth, parent, test.Name)
        {
            m_Test = test;

            if (test.TypeInfo != null)
            {
                type = test.TypeInfo.Type;
            }
            if (test.Method != null)
            {
                method = test.Method.MethodInfo;
            }

            displayName = test.Name.Replace("\n", "");
            icon = Icons.s_UnknownImg;
        }

        private static int GetId(ITestAdaptor test)
        {
            return test.UniqueName.GetHashCode();
        }

        public void SetResult(TestRunnerResult testResult)
        {
            result = testResult;
            result.SetResultChangedCallback(ResultUpdated);
            ResultUpdated(result);
        }

        public string GetResultText()
        {
            if (result.resultStatus == TestRunnerResult.ResultStatus.NotRun)
            {
                return string.Empty;
            }
            var durationString = String.Format("{0:0.000}", result.duration);
            var sb = new StringBuilder(string.Format("{0} ({1}s)", displayName.Trim(), durationString));
            if (!string.IsNullOrEmpty(result.description))
            {
                sb.AppendFormat("\n{0}", result.description);
            }
            if (!string.IsNullOrEmpty(result.messages))
            {
                sb.Append("\n---\n");
                sb.Append(result.messages.Trim());
            }
            if (!string.IsNullOrEmpty(result.stacktrace))
            {
                sb.Append("\n---\n");
                sb.Append(result.stacktrace.Trim());
            }
            if (!string.IsNullOrEmpty(result.output))
            {
                sb.Append("\n---\n");
                sb.Append(result.output.Trim());
            }
            if (sb.Length > k_ResultTestMaxLength)
            {
                sb.Length = k_ResultTestMaxLength;
                sb.AppendFormat("...\n\n---MESSAGE TRUNCATED AT {0} CHARACTERS---", k_ResultTestMaxLength);
            }
            return sb.ToString().Trim();
        }

        private void ResultUpdated(TestRunnerResult testResult)
        {
            switch (testResult.resultStatus)
            {
                case TestRunnerResult.ResultStatus.Passed:
                    icon = Icons.s_SuccessImg;
                    break;
                case TestRunnerResult.ResultStatus.Failed:
                    icon = Icons.s_FailImg;
                    break;
                case TestRunnerResult.ResultStatus.Inconclusive:
                    icon = Icons.s_InconclusiveImg;
                    break;
                case TestRunnerResult.ResultStatus.Skipped:
                    icon = Icons.s_IgnoreImg;
                    break;
                default:
                    if (testResult.ignoredOrSkipped)
                    {
                        icon = Icons.s_IgnoreImg;
                    }
                    else if (testResult.notRunnable)
                    {
                        icon = Icons.s_FailImg;
                    }
                    else
                    {
                        icon = Icons.s_UnknownImg;
                    }
                    break;
            }
        }
    }
}
