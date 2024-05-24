using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

namespace UnityEditor.Events
{
    [CustomPreview(typeof(GameObject))]
    /// <summary>
    ///   Custom preview drawing that will draw the layout properties of a given object.
    /// </summary>
    class LayoutPropertiesPreview : ObjectPreview
    {
        private const float kLabelWidth = 110;
        private const float kValueWidth = 100;

        class Styles
        {
            public GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            public GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);

            public Styles()
            {
                labelStyle.padding.right += 4;
                headerStyle.padding.right += 4;
            }
        }

        private GUIContent m_Title;
        private Styles m_Styles;

        public override void Initialize(UnityEngine.Object[] targets)
        {
            base.Initialize(targets);
        }

        public override GUIContent GetPreviewTitle()
        {
            if (m_Title == null)
            {
                m_Title = EditorGUIUtility.TrTextContent("Layout Properties");
            }
            return m_Title;
        }

        public override bool HasPreviewGUI()
        {
            GameObject go = target as GameObject;
            if (!go)
                return false;

            // Prevent allocations in the editor by using TryGetComponent
            ILayoutElement layoutElement;
            return go.TryGetComponent(out layoutElement);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (m_Styles == null)
                m_Styles = new Styles();

            GameObject go = target as GameObject;
            RectTransform rect = go.transform as RectTransform;
            if (rect == null)
                return;

            // Apply padding
            RectOffset previewPadding = new RectOffset(-5, -5, -5, -5);
            r = previewPadding.Add(r);

            // Prepare rects for columns
            r.height = EditorGUIUtility.singleLineHeight;
            Rect labelRect = r;
            Rect valueRect = r;
            Rect sourceRect = r;
            labelRect.width = kLabelWidth;
            valueRect.xMin += kLabelWidth;
            valueRect.width = kValueWidth;
            sourceRect.xMin += kLabelWidth + kValueWidth;

            // Headers
            GUI.Label(labelRect, "Property", m_Styles.headerStyle);
            GUI.Label(valueRect, "Value", m_Styles.headerStyle);
            GUI.Label(sourceRect, "Source", m_Styles.headerStyle);
            labelRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            valueRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            sourceRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Prepare reusable variable for out argument
            ILayoutElement source = null;

            // Show properties

            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Min Width", LayoutUtility.GetLayoutProperty(rect, e => e.minWidth, 0, out source).ToString(CultureInfo.InvariantCulture.NumberFormat), source);
            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Min Height", LayoutUtility.GetLayoutProperty(rect, e => e.minHeight, 0, out source).ToString(CultureInfo.InvariantCulture.NumberFormat), source);
            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Preferred Width", LayoutUtility.GetLayoutProperty(rect, e => e.preferredWidth, 0, out source).ToString(CultureInfo.InvariantCulture.NumberFormat), source);
            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Preferred Height", LayoutUtility.GetLayoutProperty(rect, e => e.preferredHeight, 0, out source).ToString(CultureInfo.InvariantCulture.NumberFormat), source);

            float flexible = 0;

            flexible = LayoutUtility.GetLayoutProperty(rect, e => e.flexibleWidth, 0, out source);
            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Flexible Width", flexible > 0 ? ("enabled (" + flexible.ToString(CultureInfo.InvariantCulture.NumberFormat) + ")") : "disabled", source);
            flexible = LayoutUtility.GetLayoutProperty(rect, e => e.flexibleHeight, 0, out source);
            ShowProp(ref labelRect, ref valueRect, ref sourceRect, "Flexible Height", flexible > 0 ? ("enabled (" + flexible.ToString(CultureInfo.InvariantCulture.NumberFormat) + ")") : "disabled", source);

            if (!rect.GetComponent<LayoutElement>())
            {
                Rect noteRect = new Rect(labelRect.x, labelRect.y + 10, r.width, EditorGUIUtility.singleLineHeight);
                GUI.Label(noteRect, "Add a LayoutElement to override values.", m_Styles.labelStyle);
            }
        }

        private void ShowProp(ref Rect labelRect, ref Rect valueRect, ref Rect sourceRect, string label, string value, ILayoutElement source)
        {
            GUI.Label(labelRect, label, m_Styles.labelStyle);
            GUI.Label(valueRect, value, m_Styles.labelStyle);
            GUI.Label(sourceRect, source == null ? "none" : source.GetType().Name, m_Styles.labelStyle);
            labelRect.y += EditorGUIUtility.singleLineHeight;
            valueRect.y += EditorGUIUtility.singleLineHeight;
            sourceRect.y += EditorGUIUtility.singleLineHeight;
        }
    }
}
