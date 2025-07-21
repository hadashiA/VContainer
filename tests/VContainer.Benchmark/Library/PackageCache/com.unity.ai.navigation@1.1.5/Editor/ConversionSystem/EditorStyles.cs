#if UNITY_2022_2_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor.Converter
{
    internal static class EditorStyles
    {
        public static Texture iconHelp;
        public static Texture2D iconPending;
        public static Texture2D iconWarn;
        public static Texture2D iconFail;
        public static Texture2D iconSuccess;

        static EditorStyles()
        {
            iconFail = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
            iconWarn = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
            iconHelp = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
            iconSuccess = EditorGUIUtility.FindTexture("TestPassed");
            iconPending = EditorGUIUtility.FindTexture("Toolbar Minus");
        }
    }
}
#endif
