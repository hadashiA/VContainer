using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    [CustomEditor(typeof(PhysicsRaycaster), true)]
    /// <summary>
    /// Custom Editor for the EventSystem Component.
    /// Extend this class to write a custom editor for a component derived from EventSystem.
    /// </summary>
    public class PhysicsRaycasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if !PACKAGE_PHYSICS
            EditorGUILayout.HelpBox("Physics module is not present. This Raycaster will have no effect", MessageType.Warning);
#endif
        }
    }
}
