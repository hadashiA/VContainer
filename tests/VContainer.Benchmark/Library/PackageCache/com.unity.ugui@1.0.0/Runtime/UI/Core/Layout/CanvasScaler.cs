using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
    [ExecuteAlways]
    [AddComponentMenu("Layout/Canvas Scaler", 101)]
    [DisallowMultipleComponent]
    /// <summary>
    ///   The Canvas Scaler component is used for controlling the overall scale and pixel density of UI elements in the Canvas. This scaling affects everything under the Canvas, including font sizes and image borders.
    /// </summary>
    /// <remarks>
    /// For a Canvas set to 'Screen Space - Overlay' or 'Screen Space - Camera', the Canvas Scaler UI Scale Mode can be set to Constant Pixel Size, Scale With Screen Size, or Constant Physical Size.
    ///
    /// Using the Constant Pixel Size mode, positions and sizes of UI elements are specified in pixels on the screen. This is also the default functionality of the Canvas when no Canvas Scaler is attached. However, With the Scale Factor setting in the Canvas Scaler, a constant scaling can be applied to all UI elements in the Canvas.
    ///
    /// Using the Scale With Screen Size mode, positions and sizes can be specified according to the pixels of a specified reference resolution. If the current screen resolution is larger than the reference resolution, the Canvas will keep having only the resolution of the reference resolution, but will scale up in order to fit the screen. If the current screen resolution is smaller than the reference resolution, the Canvas will similarly be scaled down to fit. If the current screen resolution has a different aspect ratio than the reference resolution, scaling each axis individually to fit the screen would result in non-uniform scaling, which is generally undesirable. Instead of this, the ReferenceResolution component will make the Canvas resolution deviate from the reference resolution in order to respect the aspect ratio of the screen. It is possible to control how this deviation should behave using the ::ref::screenMatchMode setting.
    ///
    /// Using the Constant Physical Size mode, positions and sizes of UI elements are specified in physical units, such as millimeters, points, or picas. This mode relies on the device reporting its screen DPI correctly. You can specify a fallback DPI to use for devices that do not report a DPI.
    ///
    /// For a Canvas set to 'World Space' the Canvas Scaler can be used to control the pixel density of UI elements in the Canvas.
    /// </remarks>
    public class CanvasScaler : UIBehaviour
    {
        /// <summary>
        /// Determines how UI elements in the Canvas are scaled.
        /// </summary>
        public enum ScaleMode
        {
            /// <summary>
            /// Using the Constant Pixel Size mode, positions and sizes of UI elements are specified in pixels on the screen.
            /// </summary>
            ConstantPixelSize,
            /// <summary>
            /// Using the Scale With Screen Size mode, positions and sizes can be specified according to the pixels of a specified reference resolution.
            /// If the current screen resolution is larger than the reference resolution, the Canvas will keep having only the resolution of the reference resolution, but will scale up in order to fit the screen. If the current screen resolution is smaller than the reference resolution, the Canvas will similarly be scaled down to fit.
            /// </summary>
            ScaleWithScreenSize,
            /// <summary>
            /// Using the Constant Physical Size mode, positions and sizes of UI elements are specified in physical units, such as millimeters, points, or picas.
            /// </summary>
            ConstantPhysicalSize
        }

        [Tooltip("Determines how UI elements in the Canvas are scaled.")]
        [SerializeField] private ScaleMode m_UiScaleMode = ScaleMode.ConstantPixelSize;

        ///<summary>
        ///Determines how UI elements in the Canvas are scaled.
        ///</summary>
        public ScaleMode uiScaleMode { get { return m_UiScaleMode; } set { m_UiScaleMode = value; } }

        [Tooltip("If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.")]
        [SerializeField] protected float m_ReferencePixelsPerUnit = 100;

        /// <summary>
        /// If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.
        /// </summary>
        public float referencePixelsPerUnit { get { return m_ReferencePixelsPerUnit; } set { m_ReferencePixelsPerUnit = value; } }


        // Constant Pixel Size settings

        [Tooltip("Scales all UI elements in the Canvas by this factor.")]
        [SerializeField] protected float m_ScaleFactor = 1;

        /// <summary>
        /// Scales all UI elements in the Canvas by this factor.
        /// </summary>

        /// <summary>
        /// Scales all UI elements in the Canvas by this factor.
        /// </summary>
        public float scaleFactor { get { return m_ScaleFactor; } set { m_ScaleFactor = Mathf.Max(0.01f, value); } }

        /// Scale the canvas area with the width as reference, the height as reference, or something in between.
        /// <summary>
        /// Scale the canvas area with the width as reference, the height as reference, or something in between.
        /// </summary>
        public enum ScreenMatchMode
        {
            /// <summary>
            /// Scale the canvas area with the width as reference, the height as reference, or something in between.
            /// </summary>
            MatchWidthOrHeight = 0,
            /// <summary>
            /// Expand the canvas area either horizontally or vertically, so the size of the canvas will never be smaller than the reference.
            /// </summary>
            Expand = 1,
            /// <summary>
            /// Crop the canvas area either horizontally or vertically, so the size of the canvas will never be larger than the reference.
            /// </summary>
            Shrink = 2
        }

        [Tooltip("The resolution the UI layout is designed for. If the screen resolution is larger, the UI will be scaled up, and if it's smaller, the UI will be scaled down. This is done in accordance with the Screen Match Mode.")]
        [SerializeField] protected Vector2 m_ReferenceResolution = new Vector2(800, 600);

        /// <summary>
        /// The resolution the UI layout is designed for.
        /// </summary>
        /// <remarks>
        /// If the screen resolution is larger, the UI will be scaled up, and if it's smaller, the UI will be scaled down. This is done in accordance with the Screen Match Mode.
        /// </remarks>
        public Vector2 referenceResolution
        {
            get
            {
                return m_ReferenceResolution;
            }
            set
            {
                m_ReferenceResolution = value;

                const float k_MinimumResolution = 0.00001f;

                if (m_ReferenceResolution.x > -k_MinimumResolution && m_ReferenceResolution.x < k_MinimumResolution) m_ReferenceResolution.x = k_MinimumResolution * Mathf.Sign(m_ReferenceResolution.x);
                if (m_ReferenceResolution.y > -k_MinimumResolution && m_ReferenceResolution.y < k_MinimumResolution) m_ReferenceResolution.y = k_MinimumResolution * Mathf.Sign(m_ReferenceResolution.y);
            }
        }

        [Tooltip("A mode used to scale the canvas area if the aspect ratio of the current resolution doesn't fit the reference resolution.")]
        [SerializeField] protected ScreenMatchMode m_ScreenMatchMode = ScreenMatchMode.MatchWidthOrHeight;
        /// <summary>
        /// A mode used to scale the canvas area if the aspect ratio of the current resolution doesn't fit the reference resolution.
        /// </summary>
        public ScreenMatchMode screenMatchMode { get { return m_ScreenMatchMode; } set { m_ScreenMatchMode = value; } }

        [Tooltip("Determines if the scaling is using the width or height as reference, or a mix in between.")]
        [Range(0, 1)]
        [SerializeField] protected float m_MatchWidthOrHeight = 0;

        /// <summary>
        /// Setting to scale the Canvas to match the width or height of the reference resolution, or a combination.
        /// </summary>
        /// <remarks>
        /// If the setting is set to 0, the Canvas is scaled according to the difference between the current screen resolution width and the reference resolution width. If the setting is set to 1, the Canvas is scaled according to the difference between the current screen resolution height and the reference resolution height.
        ///
        /// For values in between 0 and 1, the scaling is based on a combination of the relative width and height.
        ///
        /// Consider an example where the reference resolution of 640x480, and the current screen resolution is a landscape mode of 480x640.
        ///
        /// If the scaleWidthOrHeight setting is set to 0, the Canvas is scaled by 0.75 because the current resolution width of 480 is 0.75 times the reference resolution width of 640. The Canvas resolution gets a resolution of 640x853.33. This resolution has the same width as the reference resolution width, but has the aspect ratio of the current screen resolution. Note that the Canvas resolution of 640x853.33 is the current screen resolution divided by the scale factor of 0.75.
        ///
        /// If the scaleWidthOrHeight setting is set to 1, the Canvas is scaled by 1.33 because the current resolution height of 640 is 1.33 times the reference resolution height of 480. The Canvas resolution gets a resolution of 360x480. This resolution has the same height as the reference resolution width, but has the aspect ratio of the current screen resolution. Note that the Canvas resolution of 360x480 is the current screen resolution divided by the scale factor of 1.33.
        ///
        /// If the scaleWidthOrHeight setting is set to 0.5, we find the horizontal scaling needed (0.75) and the vertical scaling needed (1.33) and find the average. However, we do the average in logarithmic space. A regular average of 0.75 and 1.33 would produce a result of 1.04. However, since multiplying by 1.33 is the same as diving by 0.75, the two scale factor really corresponds to multiplying by 0.75 versus dividing by 0.75, and the average of those two things should even out and produce a neutral result. The average in logarithmic space of 0.75 and 1.33 is exactly 1.0, which is what we want. The Canvas resolution hence ends up being 480x640 which is the current resolution divided by the scale factor of 1.0.
        ///
        /// The logic works the same for all values. The average between the horizontal and vertical scale factor is a weighted average based on the matchWidthOrHeight value.
        /// </remarks>
        public float matchWidthOrHeight { get { return m_MatchWidthOrHeight; } set { m_MatchWidthOrHeight = value; } }

        // The log base doesn't have any influence on the results whatsoever, as long as the same base is used everywhere.
        private const float kLogBase = 2;

        /// <summary>
        /// The possible physical unit types
        /// </summary>
        public enum Unit
        {
            /// <summary>
            /// Use centimeters.
            /// A centimeter is 1/100 of a meter
            /// </summary>
            Centimeters,
            /// <summary>
            /// Use millimeters.
            /// A millimeter is 1/10 of a centimeter, and 1/1000 of a meter.
            /// </summary>
            Millimeters,
            /// <summary>
            /// Use inches.
            /// </summary>
            Inches,
            /// <summary>
            /// Use points.
            /// One point is 1/12 of a pica, and 1/72 of an inch.
            /// </summary>
            Points,
            /// <summary>
            /// Use picas.
            /// One pica is 1/6 of an inch.
            /// </summary>
            Picas
        }

        [Tooltip("The physical unit to specify positions and sizes in.")]
        [SerializeField] protected Unit m_PhysicalUnit = Unit.Points;

        /// <summary>
        /// The physical unit to specify positions and sizes in.
        /// </summary>
        public Unit physicalUnit { get { return m_PhysicalUnit; } set { m_PhysicalUnit = value; } }

        [Tooltip("The DPI to assume if the screen DPI is not known.")]
        [SerializeField] protected float m_FallbackScreenDPI = 96;

        /// <summary>
        /// The DPI to assume if the screen DPI is not known.
        /// </summary>
        public float fallbackScreenDPI { get { return m_FallbackScreenDPI; } set { m_FallbackScreenDPI = value; } }

        [Tooltip("The pixels per inch to use for sprites that have a 'Pixels Per Unit' setting that matches the 'Reference Pixels Per Unit' setting.")]
        [SerializeField] protected float m_DefaultSpriteDPI = 96;

        /// <summary>
        /// The pixels per inch to use for sprites that have a 'Pixels Per Unit' setting that matches the 'Reference Pixels Per Unit' setting.
        /// </summary>
        public float defaultSpriteDPI { get { return m_DefaultSpriteDPI; } set { m_DefaultSpriteDPI = Mathf.Max(1, value); } }


        // World Canvas settings

        [Tooltip("The amount of pixels per unit to use for dynamically created bitmaps in the UI, such as Text.")]
        [SerializeField] protected float m_DynamicPixelsPerUnit = 1;

        /// <summary>
        /// The amount of pixels per unit to use for dynamically created bitmaps in the UI, such as Text.
        /// </summary>
        public float dynamicPixelsPerUnit { get { return m_DynamicPixelsPerUnit; } set { m_DynamicPixelsPerUnit = value; } }


        // General variables

        private Canvas m_Canvas;
        [System.NonSerialized]
        private float m_PrevScaleFactor = 1;
        [System.NonSerialized]
        private float m_PrevReferencePixelsPerUnit = 100;

        [SerializeField] protected bool m_PresetInfoIsWorld = false;

        protected CanvasScaler() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Canvas = GetComponent<Canvas>();
            Handle();
            Canvas.preWillRenderCanvases += Canvas_preWillRenderCanvases;
        }

        private void Canvas_preWillRenderCanvases()
        {
            Handle();
        }

        protected override void OnDisable()
        {
            SetScaleFactor(1);
            SetReferencePixelsPerUnit(100);
            Canvas.preWillRenderCanvases -= Canvas_preWillRenderCanvases;
            base.OnDisable();
        }

        ///<summary>
        ///Method that handles calculations of canvas scaling.
        ///</summary>
        protected virtual void Handle()
        {
            if (m_Canvas == null || !m_Canvas.isRootCanvas)
                return;

            if (m_Canvas.renderMode == RenderMode.WorldSpace)
            {
                HandleWorldCanvas();
                return;
            }

            switch (m_UiScaleMode)
            {
                case ScaleMode.ConstantPixelSize: HandleConstantPixelSize(); break;
                case ScaleMode.ScaleWithScreenSize: HandleScaleWithScreenSize(); break;
                case ScaleMode.ConstantPhysicalSize: HandleConstantPhysicalSize(); break;
            }
        }

        /// <summary>
        /// Handles canvas scaling for world canvas.
        /// </summary>
        protected virtual void HandleWorldCanvas()
        {
            SetScaleFactor(m_DynamicPixelsPerUnit);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        /// <summary>
        /// Handles canvas scaling for a constant pixel size.
        /// </summary>
        protected virtual void HandleConstantPixelSize()
        {
            SetScaleFactor(m_ScaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        /// <summary>
        /// Handles canvas scaling that scales with the screen size.
        /// </summary>
        protected virtual void HandleScaleWithScreenSize()
        {
            Vector2 screenSize = m_Canvas.renderingDisplaySize;

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            int displayIndex = m_Canvas.targetDisplay;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                Display disp = Display.displays[displayIndex];
                screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
            }


            float scaleFactor = 0;
            switch (m_ScreenMatchMode)
            {
                case ScreenMatchMode.MatchWidthOrHeight:
                {
                    // We take the log of the relative width and height before taking the average.
                    // Then we transform it back in the original space.
                    // the reason to transform in and out of logarithmic space is to have better behavior.
                    // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                    // In normal space the average would be (0.5 + 2) / 2 = 1.25
                    // In logarithmic space the average is (-1 + 1) / 2 = 0
                    float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
                    float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
                    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                    scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                    break;
                }
                case ScreenMatchMode.Expand:
                {
                    scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }
                case ScreenMatchMode.Shrink:
                {
                    scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                    break;
                }
            }

            SetScaleFactor(scaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        ///<summary>
        ///Handles canvas scaling for a constant physical size.
        ///</summary>
        protected virtual void HandleConstantPhysicalSize()
        {
            float currentDpi = Screen.dpi;
            float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
            float targetDPI = 1;
            switch (m_PhysicalUnit)
            {
                case Unit.Centimeters: targetDPI = 2.54f; break;
                case Unit.Millimeters: targetDPI = 25.4f; break;
                case Unit.Inches:      targetDPI =     1; break;
                case Unit.Points:      targetDPI =    72; break;
                case Unit.Picas:       targetDPI =     6; break;
            }

            SetScaleFactor(dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
        }

        /// <summary>
        /// Sets the scale factor on the canvas.
        /// </summary>
        /// <param name="scaleFactor">The scale factor to use.</param>
        protected void SetScaleFactor(float scaleFactor)
        {
            if (scaleFactor == m_PrevScaleFactor)
                return;

            m_Canvas.scaleFactor = scaleFactor;
            m_PrevScaleFactor = scaleFactor;
        }

        /// <summary>
        /// Sets the referencePixelsPerUnit on the Canvas.
        /// </summary>
        /// <param name="referencePixelsPerUnit">The new reference pixels per Unity value</param>
        protected void SetReferencePixelsPerUnit(float referencePixelsPerUnit)
        {
            if (referencePixelsPerUnit == m_PrevReferencePixelsPerUnit)
                return;

            m_Canvas.referencePixelsPerUnit = referencePixelsPerUnit;
            m_PrevReferencePixelsPerUnit = referencePixelsPerUnit;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_ScaleFactor = Mathf.Max(0.01f, m_ScaleFactor);
            m_DefaultSpriteDPI = Mathf.Max(1, m_DefaultSpriteDPI);
        }

#endif
    }
}
