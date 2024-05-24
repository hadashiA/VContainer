using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// Base class for custom editors that are for components that implement the SelfControllerEditor interface.
    /// </summary>
    public class SelfControllerEditor : Editor
    {
        static string s_Warning = "Parent has a type of layout group component. A child of a layout group should not have a {0} component, since it should be driven by the layout group.";

        public override void OnInspectorGUI()
        {
            bool anyHaveLayoutParent = false;
            for (int i = 0; i < targets.Length; i++)
            {
                Component comp = (targets[i] as Component);
                ILayoutIgnorer ignorer = comp.GetComponent(typeof(ILayoutIgnorer)) as ILayoutIgnorer;
                if (ignorer != null && ignorer.ignoreLayout)
                    continue;

                RectTransform parent = comp.transform.parent as RectTransform;
                if (parent != null)
                {
                    Behaviour layoutGroup = parent.GetComponent(typeof(ILayoutGroup)) as Behaviour;
                    if (layoutGroup != null && layoutGroup.enabled)
                    {
                        anyHaveLayoutParent = true;
                        break;
                    }
                }
            }
            if (anyHaveLayoutParent)
                EditorGUILayout.HelpBox(string.Format(s_Warning, ObjectNames.NicifyVariableName(target.GetType().Name)), MessageType.Warning);
        }
    }
}
