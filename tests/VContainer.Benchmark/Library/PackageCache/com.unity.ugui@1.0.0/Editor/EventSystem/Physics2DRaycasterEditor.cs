using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    [CustomEditor(typeof(Physics2DRaycaster), true)]
    /// <summary>
    /// Custom Editor for the EventSystem Component.
    /// Extend this class to write a custom editor for a component derived from EventSystem.
    /// </summary>
    public class Physics2DRaycasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if !PACKAGE_PHYSICS2D
            EditorGUILayout.HelpBox("Physics2D module is not present. This Raycaster will have no effect", MessageType.Warning);
#endif
        }
    }
}
