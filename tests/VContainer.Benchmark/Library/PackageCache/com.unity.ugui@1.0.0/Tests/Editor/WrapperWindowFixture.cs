using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WrapperWindowFixture
{
    private static WrapperWindow s_MostRecentWrapperWindow;

    public class WrapperWindow : EditorWindow
    {
        // Return true to end the test
        public Func<WrapperWindow, bool> onGUIDelegate;

        public bool TestCompleted { get; set; }

        public void OnGUI()
        {
            if (onGUIDelegate != null)
            {
                TestCompleted = onGUIDelegate.Invoke(this);
                if (TestCompleted)
                    onGUIDelegate = null;
            }
        }
    }

    public static WrapperWindow GetWindow(Func<WrapperWindow, bool> onGUIDelegate, bool utility = false)
    {
        return GetWindow(onGUIDelegate, 800, 600, utility);
    }

    public static WrapperWindow GetWindow(Func<WrapperWindow, bool> onGUIDelegate, int width, int height, bool utility = false)
    {
        WrapperWindow window;
        if (utility)
            window = EditorWindow.GetWindow<WrapperWindow>(true);
        else
        {
            window = ScriptableObject.CreateInstance<WrapperWindow>();
            window.hideFlags = HideFlags.DontSave;
        }

        window.onGUIDelegate = onGUIDelegate;
        window.position = new Rect(0, 0, width, height);
        window.Show();
        s_MostRecentWrapperWindow = window;
        return window;
    }

    [TearDown]
    public void CloseMostRecentWrapperWindow()
    {
        if (s_MostRecentWrapperWindow == null)
            return;
        s_MostRecentWrapperWindow.Close();
        s_MostRecentWrapperWindow = null;
    }

    protected static void RegisterWrapperWindow(WrapperWindow window)
    {
        s_MostRecentWrapperWindow = window;
    }
}
