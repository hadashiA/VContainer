using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal static class Icons
    {
        public static readonly Texture2D s_FailImg;
        public static readonly Texture2D s_IgnoreImg;
        public static readonly Texture2D s_SuccessImg;
        public static readonly Texture2D s_UnknownImg;
        public static readonly Texture2D s_InconclusiveImg;
        public static readonly Texture2D s_StopwatchImg;

        static Icons()
        {
            s_FailImg = EditorGUIUtility.IconContent("TestFailed").image as Texture2D;
            s_IgnoreImg = EditorGUIUtility.IconContent("TestIgnored").image as Texture2D;
            s_SuccessImg = EditorGUIUtility.IconContent("TestPassed").image as Texture2D;
            s_UnknownImg = EditorGUIUtility.IconContent("TestNormal").image as Texture2D;
            s_InconclusiveImg = EditorGUIUtility.IconContent("TestInconclusive").image as Texture2D;
            s_StopwatchImg = EditorGUIUtility.IconContent("TestStopwatch").image as Texture2D;
        }
    }
}
