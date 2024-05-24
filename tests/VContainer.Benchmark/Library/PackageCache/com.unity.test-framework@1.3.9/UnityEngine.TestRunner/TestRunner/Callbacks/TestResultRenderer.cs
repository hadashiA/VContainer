using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    internal class TestResultRenderer
    {
        private static class Styles
        {
            public static readonly GUIStyle SucceedLabelStyle;
            public static readonly GUIStyle FailedLabelStyle;
            public static readonly GUIStyle FailedMessagesStyle;

            static Styles()
            {
                SucceedLabelStyle = new GUIStyle("label");
                SucceedLabelStyle.normal.textColor = Color.green;
                SucceedLabelStyle.fontSize = 48;

                FailedLabelStyle = new GUIStyle("label");
                FailedLabelStyle.normal.textColor = Color.red;
                FailedLabelStyle.fontSize = 32;

                FailedMessagesStyle = new GUIStyle("label");
                FailedMessagesStyle.wordWrap = false;
                FailedMessagesStyle.richText = true;
            }
        }

        private readonly List<ITestResult> m_FailedTestCollection;

        private bool m_ShowResults;
        private Vector2 m_ScrollPosition;

        public TestResultRenderer(ITestResult testResults)
        {
            m_FailedTestCollection = new List<ITestResult>();
            GetFailedTests(testResults);
        }

        private void GetFailedTests(ITestResult testResults)
        {
            if (testResults is TestCaseResult)
            {
                if (testResults.ResultState.Status == TestStatus.Failed)
                    m_FailedTestCollection.Add(testResults);
            }
            else if (testResults.HasChildren)
            {
                foreach (var testResultsChild in testResults.Children)
                {
                    GetFailedTests(testResultsChild);
                }
            }
        }

        private const int k_MaxStringLength = 15000;

        public void ShowResults()
        {
            m_ShowResults = true;
            Cursor.visible = true;
        }

        public void Draw()
        {
            if (!m_ShowResults) return;
            if (m_FailedTestCollection.Count == 0)
            {
                GUILayout.Label("All test(s) succeeded", Styles.SucceedLabelStyle, GUILayout.Width(600));
            }
            else
            {
                int count = m_FailedTestCollection.Count;
                GUILayout.Label(count + " tests failed!", Styles.FailedLabelStyle);

                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandWidth(true));
                var text = "";

                text += "<b><size=18>Code-based tests</size></b>\n";
                text += string.Join("\n", m_FailedTestCollection
                    .Select(result => result.Name + " " + result.ResultState + "\n" + result.Message)
                    .ToArray());

                if (text.Length > k_MaxStringLength)
                    text = text.Substring(0, k_MaxStringLength);

                GUILayout.TextArea(text, Styles.FailedMessagesStyle);
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("Close"))
                Application.Quit();
        }
    }
}
