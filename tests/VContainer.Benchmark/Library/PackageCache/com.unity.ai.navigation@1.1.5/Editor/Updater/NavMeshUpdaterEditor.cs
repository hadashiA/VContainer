#if UNITY_2022_2_OR_NEWER
using Unity.AI.Navigation.Editor.Converter;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Updater
{
    internal static class NavMeshUpdaterEditor
    {
        [MenuItem("Window/AI/NavMesh Updater", false, 50)]
        public static void ShowWindow()
        {
            SystemConvertersEditor wnd = EditorWindow.GetWindow<SystemConvertersEditor>();
            wnd.titleContent = new GUIContent("NavMesh Updater");
            wnd.DontSaveToLayout(wnd);
            wnd.maxSize = new Vector2(650f, 4000f);
            wnd.minSize = new Vector2(650f, 400f);
            wnd.Show();
        }
    }
}
#endif
