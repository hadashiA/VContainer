using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class TestFilterSettings
    {
        public bool showSucceeded;
        public bool showFailed;
        public bool showIgnored;
        public bool showNotRun;

        public string filterByName;
        public int filterByCategory;

        private GUIContent m_SucceededBtn;
        private GUIContent m_FailedBtn;
        private GUIContent m_IgnoredBtn;
        private GUIContent m_NotRunBtn;

        public string[] availableCategories;

        private readonly string m_PrefsKey;

        public TestFilterSettings(string prefsKey)
        {
            availableCategories = null;
            m_PrefsKey = prefsKey;
            Load();
            UpdateCounters(Enumerable.Empty<TestRunnerResult>());
        }

        public void Load()
        {
            showSucceeded = EditorPrefs.GetBool(m_PrefsKey + ".ShowSucceeded", true);
            showFailed = EditorPrefs.GetBool(m_PrefsKey + ".ShowFailed", true);
            showIgnored = EditorPrefs.GetBool(m_PrefsKey + ".ShowIgnored", true);
            showNotRun = EditorPrefs.GetBool(m_PrefsKey + ".ShowNotRun", true);
            filterByName = EditorPrefs.GetString(m_PrefsKey + ".FilterByName", string.Empty);
            filterByCategory = EditorPrefs.GetInt(m_PrefsKey + ".FilterByCategory", 0);
        }

        public void Save()
        {
            EditorPrefs.SetBool(m_PrefsKey + ".ShowSucceeded", showSucceeded);
            EditorPrefs.SetBool(m_PrefsKey + ".ShowFailed", showFailed);
            EditorPrefs.SetBool(m_PrefsKey + ".ShowIgnored", showIgnored);
            EditorPrefs.SetBool(m_PrefsKey + ".ShowNotRun", showNotRun);
            EditorPrefs.SetString(m_PrefsKey + ".FilterByName", filterByName);
            EditorPrefs.SetInt(m_PrefsKey + ".FilterByCategory", filterByCategory);
        }

        public void UpdateCounters(IEnumerable<TestRunnerResult> results)
        {
            var summary = new ResultSummarizer(results);

            m_SucceededBtn = new GUIContent(summary.Passed.ToString(), Icons.s_SuccessImg, "Show tests that succeeded");
            m_FailedBtn = new GUIContent((summary.errors + summary.failures + summary.inconclusive).ToString(), Icons.s_FailImg, "Show tests that failed");
            m_IgnoredBtn = new GUIContent((summary.ignored + summary.notRunnable).ToString(), Icons.s_IgnoreImg, "Show tests that are ignored");
            m_NotRunBtn = new GUIContent((summary.testsNotRun - summary.ignored - summary.notRunnable).ToString(), Icons.s_UnknownImg, "Show tests that didn't run");
        }

        public string[] GetSelectedCategories()
        {
            if (availableCategories == null)
                return new string[0];

            return availableCategories.Where((c, i) => (filterByCategory & (1 << i)) != 0).ToArray();
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            filterByName = GUILayout.TextField(filterByName, "ToolbarSeachTextField", GUILayout.MinWidth(100), GUILayout.MaxWidth(250), GUILayout.ExpandWidth(true));
            if (GUILayout.Button(GUIContent.none, string.IsNullOrEmpty(filterByName) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton"))
                filterByName = string.Empty;

            if (availableCategories != null && availableCategories.Length > 0)
                filterByCategory = EditorGUILayout.MaskField(filterByCategory, availableCategories, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(90));

            showSucceeded = GUILayout.Toggle(showSucceeded, m_SucceededBtn, EditorStyles.toolbarButton);
            showFailed = GUILayout.Toggle(showFailed, m_FailedBtn, EditorStyles.toolbarButton);
            showIgnored = GUILayout.Toggle(showIgnored, m_IgnoredBtn, EditorStyles.toolbarButton);
            showNotRun = GUILayout.Toggle(showNotRun, m_NotRunBtn, EditorStyles.toolbarButton);

            if (EditorGUI.EndChangeCheck())
                Save();
        }

        public RenderingOptions BuildRenderingOptions()
        {
            var options = new RenderingOptions();
            options.showSucceeded = showSucceeded;
            options.showFailed = showFailed;
            options.showIgnored = showIgnored;
            options.showNotRunned = showNotRun;
            options.nameFilter = filterByName;
            options.categories = GetSelectedCategories();
            return options;
        }
    }
}
