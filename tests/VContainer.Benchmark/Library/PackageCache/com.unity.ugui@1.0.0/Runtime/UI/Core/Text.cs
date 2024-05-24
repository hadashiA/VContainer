using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Legacy/Text", 100)]
    /// <summary>
    /// The default Graphic to draw font data to screen.
    /// </summary>
    public class Text : MaskableGraphic, ILayoutElement
    {
        [SerializeField] private FontData m_FontData = FontData.defaultFontData;

#if UNITY_EDITOR
        // needed to track font changes from the inspector
        private Font m_LastTrackedFont;
#endif

        [TextArea(3, 10)][SerializeField] protected string m_Text = String.Empty;

        private TextGenerator m_TextCache;
        private TextGenerator m_TextCacheForLayout;

        static protected Material s_DefaultText = null;

        // We use this flag instead of Unregistering/Registering the callback to avoid allocation.
        [NonSerialized] protected bool m_DisableFontTextureRebuiltCallback = false;

        protected Text()
        {
            useLegacyMeshGeneration = false;
        }

        /// <summary>
        /// The cached TextGenerator used when generating visible Text.
        /// </summary>

        public TextGenerator cachedTextGenerator
        {
            get { return m_TextCache ?? (m_TextCache = (m_Text.Length != 0 ? new TextGenerator(m_Text.Length) : new TextGenerator())); }
        }

        /// <summary>
        /// The cached TextGenerator used when determine Layout
        /// </summary>
        public TextGenerator cachedTextGeneratorForLayout
        {
            get { return m_TextCacheForLayout ?? (m_TextCacheForLayout = new TextGenerator()); }
        }

        /// <summary>
        /// Text's texture comes from the font.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (font != null && font.material != null && font.material.mainTexture != null)
                    return font.material.mainTexture;

                if (m_Material != null)
                    return m_Material.mainTexture;

                return base.mainTexture;
            }
        }

        /// <summary>
        /// Called by the FontUpdateTracker when the texture associated with a font is modified.
        /// </summary>
        public void FontTextureChanged()
        {
            // Only invoke if we are not destroyed.
            if (!this)
                return;

            if (m_DisableFontTextureRebuiltCallback)
                return;

            cachedTextGenerator.Invalidate();

            if (!IsActive())
                return;

            // this is a bit hacky, but it is currently the
            // cleanest solution....
            // if we detect the font texture has changed and are in a rebuild loop
            // we just regenerate the verts for the new UV's
            if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
                UpdateGeometry();
            else
            {
                SetAllDirty();
            }
        }

        /// <summary>
        /// The Font used by the text.
        /// </summary>
        /// <remarks>
        /// This is the font used by the Text component. Use it to alter or return the font from the Text. There are many free fonts available online.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Create a new Text GameObject by going to Create>UI>Text in the Editor. Attach this script to the Text GameObject. Then, choose or click and drag your own font into the Font section in the Inspector window.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class TextFontExample : MonoBehaviour
        /// {
        ///     Text m_Text;
        ///     //Attach your own Font in the Inspector
        ///     public Font m_Font;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Text component from the GameObject
        ///         m_Text = GetComponent<Text>();
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         if (Input.GetKey(KeyCode.Space))
        ///         {
        ///             //Change the Text Font to the Font attached in the Inspector
        ///             m_Text.font = m_Font;
        ///             //Change the Text to the message below
        ///             m_Text.text = "My Font Changed!";
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Font font
        {
            get
            {
                return m_FontData.font;
            }
            set
            {
                if (m_FontData.font == value)
                    return;

                if (isActiveAndEnabled)
                    FontUpdateTracker.UntrackText(this);

                m_FontData.font = value;

                if (isActiveAndEnabled)
                    FontUpdateTracker.TrackText(this);

#if UNITY_EDITOR
                // needed to track font changes from the inspector
                m_LastTrackedFont = value;
#endif

                SetAllDirty();
            }
        }

        /// <summary>
        /// Text that's being displayed by the Text.
        /// </summary>
        /// <remarks>
        /// This is the string value of a Text component. Use this to read or edit the message displayed in Text.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Text m_MyText;
        ///
        ///     void Start()
        ///     {
        ///         //Text sets your text to say this message
        ///         m_MyText.text = "This is my text";
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //Press the space key to change the Text message
        ///         if (Input.GetKey(KeyCode.Space))
        ///         {
        ///             m_MyText.text = "My text has now changed.";
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (String.IsNullOrEmpty(m_Text))
                        return;
                    m_Text = "";
                    SetVerticesDirty();
                }
                else if (m_Text != value)
                {
                    m_Text = value;
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// Whether this Text will support rich text.
        /// </summary>

        public bool supportRichText
        {
            get
            {
                return m_FontData.richText;
            }
            set
            {
                if (m_FontData.richText == value)
                    return;
                m_FontData.richText = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Should the text be allowed to auto resized.
        /// </summary>

        public bool resizeTextForBestFit
        {
            get
            {
                return m_FontData.bestFit;
            }
            set
            {
                if (m_FontData.bestFit == value)
                    return;
                m_FontData.bestFit = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// The minimum size the text is allowed to be.
        /// </summary>
        public int resizeTextMinSize
        {
            get
            {
                return m_FontData.minSize;
            }
            set
            {
                if (m_FontData.minSize == value)
                    return;
                m_FontData.minSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// The maximum size the text is allowed to be. 1 = infinitely large.
        /// </summary>
        public int resizeTextMaxSize
        {
            get
            {
                return m_FontData.maxSize;
            }
            set
            {
                if (m_FontData.maxSize == value)
                    return;
                m_FontData.maxSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// The positioning of the text reliative to its [[RectTransform]].
        /// </summary>
        /// <remarks>
        /// This is the positioning of the Text relative to its RectTransform. You can alter this via script or in the Inspector of a Text component using the buttons in the Alignment section.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Create a Text GameObject by going to __Create__>__UI__>__Text__. Attach this script to the GameObject to see it working.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class UITextAlignment : MonoBehaviour
        /// {
        ///     Text m_Text;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Text Component
        ///         m_Text = GetComponent<Text>();
        ///         //Switch the Text alignment to the middle
        ///         m_Text.alignment = TextAnchor.MiddleCenter;
        ///     }
        ///
        /// //This is a legacy function used for an instant demonstration. See the <a href="https://unity3d.com/learn/tutorials/s/user-interface-ui">UI Tutorials pages </a> and [[wiki:UISystem|UI Section]] of the manual for more information on creating your own buttons etc.
        ///     void OnGUI()
        ///     {
        ///         //Press this Button to change the Text alignment to the lower right
        ///         if (GUI.Button(new Rect(0, 0, 100, 40), "Lower Right"))
        ///         {
        ///             m_Text.alignment = TextAnchor.LowerRight;
        ///         }
        ///
        ///         //Press this Button to change the Text alignment to the upper left
        ///         if (GUI.Button(new Rect(150, 0, 100, 40), "Upper Left"))
        ///         {
        ///             m_Text.alignment = TextAnchor.UpperLeft;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public TextAnchor alignment
        {
            get
            {
                return m_FontData.alignment;
            }
            set
            {
                if (m_FontData.alignment == value)
                    return;
                m_FontData.alignment = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Use the extents of glyph geometry to perform horizontal alignment rather than glyph metrics.
        /// </summary>
        /// <remarks>
        /// This can result in better fitting left and right alignment, but may result in incorrect positioning when attempting to overlay multiple fonts (such as a specialized outline font) on top of each other.
        /// </remarks>
        public bool alignByGeometry
        {
            get
            {
                return m_FontData.alignByGeometry;
            }
            set
            {
                if (m_FontData.alignByGeometry == value)
                    return;
                m_FontData.alignByGeometry = value;

                SetVerticesDirty();
            }
        }

        /// <summary>
        /// The size that the Font should render at. Unit of measure is Points.
        /// </summary>
        /// <remarks>
        /// This is the size of the Font of the Text. Use this to fetch or change the size of the Font. When changing the Font size, remember to take into account the RectTransform of the Text. Larger Font sizes or messages may not fit in certain rectangle sizes and do not show in the Scene.
        /// Note: Point size is not consistent from one font to another.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //For this script to work, create a new Text GameObject by going to Create>U>Text. Attach the script to the Text GameObject. Make sure the GameObject has a RectTransform component.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Text m_Text;
        ///     RectTransform m_RectTransform;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Text and RectTransform components from the GameObject
        ///         m_Text = GetComponent<Text>();
        ///         m_RectTransform = GetComponent<RectTransform>();
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //Press the space key to change the Font size
        ///         if (Input.GetKey(KeyCode.Space))
        ///         {
        ///             changeFontSize();
        ///         }
        ///     }
        ///
        ///     void changeFontSize()
        ///     {
        ///         //Change the Font Size to 16
        ///         m_Text.fontSize = 30;
        ///
        ///         //Change the RectTransform size to allow larger fonts and sentences
        ///         m_RectTransform.sizeDelta = new Vector2(m_Text.fontSize * 10, 100);
        ///
        ///         //Change the m_Text text to the message below
        ///         m_Text.text = "I changed my Font size!";
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public int fontSize
        {
            get
            {
                return m_FontData.fontSize;
            }
            set
            {
                if (m_FontData.fontSize == value)
                    return;
                m_FontData.fontSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Horizontal overflow mode.
        /// </summary>
        /// <remarks>
        /// When set to HorizontalWrapMode.Overflow, text can exceed the horizontal boundaries of the Text graphic. When set to HorizontalWrapMode.Wrap, text will be word-wrapped to fit within the boundaries.
        /// </remarks>
        public HorizontalWrapMode horizontalOverflow
        {
            get
            {
                return m_FontData.horizontalOverflow;
            }
            set
            {
                if (m_FontData.horizontalOverflow == value)
                    return;
                m_FontData.horizontalOverflow = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Vertical overflow mode.
        /// </summary>
        public VerticalWrapMode verticalOverflow
        {
            get
            {
                return m_FontData.verticalOverflow;
            }
            set
            {
                if (m_FontData.verticalOverflow == value)
                    return;
                m_FontData.verticalOverflow = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Line spacing, specified as a factor of font line height. A value of 1 will produce normal line spacing.
        /// </summary>
        public float lineSpacing
        {
            get
            {
                return m_FontData.lineSpacing;
            }
            set
            {
                if (m_FontData.lineSpacing == value)
                    return;
                m_FontData.lineSpacing = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Font style used by the Text's text.
        /// </summary>

        public FontStyle fontStyle
        {
            get
            {
                return m_FontData.fontStyle;
            }
            set
            {
                if (m_FontData.fontStyle == value)
                    return;
                m_FontData.fontStyle = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Provides information about how fonts are scale to the screen.
        /// </summary>
        /// <remarks>
        /// For dynamic fonts, the value is equivalent to the scale factor of the canvas. For non-dynamic fonts, the value is calculated from the requested text size and the size from the font.
        /// </remarks>
        public float pixelsPerUnit
        {
            get
            {
                var localCanvas = canvas;
                if (!localCanvas)
                    return 1;
                // For dynamic fonts, ensure we use one pixel per pixel on the screen.
                if (!font || font.dynamic)
                    return localCanvas.scaleFactor;
                // For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
                if (m_FontData.fontSize <= 0 || font.fontSize <= 0)
                    return 1;
                return font.fontSize / (float)m_FontData.fontSize;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            cachedTextGenerator.Invalidate();
            FontUpdateTracker.TrackText(this);
        }

        protected override void OnDisable()
        {
            FontUpdateTracker.UntrackText(this);
            base.OnDisable();
        }

        protected override void UpdateGeometry()
        {
            if (font != null)
            {
                base.UpdateGeometry();
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            AssignDefaultFontIfNecessary();
        }

#endif
        internal void AssignDefaultFont()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        internal void AssignDefaultFontIfNecessary()
        {
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>
        /// Convenience function to populate the generation setting for the text.
        /// </summary>
        /// <param name="extents">The extents the text can draw in.</param>
        /// <returns>Generated settings.</returns>
        public TextGenerationSettings GetGenerationSettings(Vector2 extents)
        {
            var settings = new TextGenerationSettings();

            settings.generationExtents = extents;
            if (font != null && font.dynamic)
            {
                settings.fontSize = m_FontData.fontSize;
                settings.resizeTextMinSize = m_FontData.minSize;
                settings.resizeTextMaxSize = m_FontData.maxSize;
            }

            // Other settings
            settings.textAnchor = m_FontData.alignment;
            settings.alignByGeometry = m_FontData.alignByGeometry;
            settings.scaleFactor = pixelsPerUnit;
            settings.color = color;
            settings.font = font;
            settings.pivot = rectTransform.pivot;
            settings.richText = m_FontData.richText;
            settings.lineSpacing = m_FontData.lineSpacing;
            settings.fontStyle = m_FontData.fontStyle;
            settings.resizeTextForBestFit = m_FontData.bestFit;
            settings.updateBounds = false;
            settings.horizontalOverflow = m_FontData.horizontalOverflow;
            settings.verticalOverflow = m_FontData.verticalOverflow;

            return settings;
        }

        /// <summary>
        /// Convenience function to determine the vector offset of the anchor.
        /// </summary>
        static public Vector2 GetTextAnchorPivot(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft:    return new Vector2(0, 0);
                case TextAnchor.LowerCenter:  return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight:   return new Vector2(1, 0);
                case TextAnchor.MiddleLeft:   return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight:  return new Vector2(1, 0.5f);
                case TextAnchor.UpperLeft:    return new Vector2(0, 1);
                case TextAnchor.UpperCenter:  return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight:   return new Vector2(1, 1);
                default: return Vector2.zero;
            }
        }

        readonly UIVertex[] m_TempVerts = new UIVertex[4];
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            int vertCount = verts.Count;

            // We have no verts to process just return (case 1037923)
            if (vertCount <= 0)
            {
                toFill.Clear();
                return;
            }

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
        }

        public virtual void CalculateLayoutInputHorizontal() {}
        public virtual void CalculateLayoutInputVertical() {}

        public virtual float minWidth
        {
            get { return 0; }
        }

        public virtual float preferredWidth
        {
            get
            {
                var settings = GetGenerationSettings(Vector2.zero);
                return cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight
        {
            get { return 0; }
        }

        public virtual float preferredHeight
        {
            get
            {
                var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
                return cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }

#if UNITY_EDITOR
        public override void OnRebuildRequested()
        {
            // After a Font asset gets re-imported the managed side gets deleted and recreated,
            // that means the delegates are not persisted.
            // so we need to properly enforce a consistent state here.
            if (isActiveAndEnabled)
            {
                FontUpdateTracker.UntrackText(this);
                FontUpdateTracker.TrackText(this);
            }

            // Also the textgenerator is no longer valid.
            cachedTextGenerator.Invalidate();

            base.OnRebuildRequested();
        }

        // The Text inspector editor can change the font, and we need a way to track changes so that we get the appropriate rebuild callbacks
        // We can intercept changes in OnValidate, and keep track of the previous font reference
        protected override void OnValidate()
        {
            if (!IsActive())
            {
                base.OnValidate();
                return;
            }

            if (m_FontData.font != m_LastTrackedFont)
            {
                Font newFont = m_FontData.font;
                m_FontData.font = m_LastTrackedFont;

                if (isActiveAndEnabled)
                    FontUpdateTracker.UntrackText(this);

                m_FontData.font = newFont;

                if (isActiveAndEnabled)
                    FontUpdateTracker.TrackText(this);

                m_LastTrackedFont = newFont;
            }
            base.OnValidate();
        }

#endif // if UNITY_EDITOR
    }
}
