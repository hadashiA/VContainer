using System.Collections.Generic;
using UnityEngine.UI.Collections;

namespace UnityEngine.UI
{
    /// <summary>
    /// Registry class to keep track of all IClippers that exist in the scene
    /// </summary>
    /// <remarks>
    /// This is used during the CanvasUpdate loop to cull clippable elements. The clipping is called after layout, but before Graphic update.
    /// </remarks>
    public class ClipperRegistry
    {
        static ClipperRegistry s_Instance;

        readonly IndexedSet<IClipper> m_Clippers = new IndexedSet<IClipper>();

        protected ClipperRegistry()
        {
            // This is needed for AOT platforms. Without it the compile doesn't get the definition of the Dictionarys
#pragma warning disable 168
            Dictionary<IClipper, int> emptyIClipperDic;
#pragma warning restore 168
        }

        /// <summary>
        /// The singleton instance of the clipper registry.
        /// </summary>
        public static ClipperRegistry instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new ClipperRegistry();
                return s_Instance;
            }
        }

        /// <summary>
        /// Perform the clipping on all registered IClipper
        /// </summary>
        public void Cull()
        {
            var clippersCount = m_Clippers.Count;
            for (var i = 0; i < clippersCount; ++i)
            {
                m_Clippers[i].PerformClipping();
            }
        }

        /// <summary>
        /// Register a unique IClipper element
        /// </summary>
        /// <param name="c">The clipper element to add</param>
        public static void Register(IClipper c)
        {
            if (c == null)
                return;
            instance.m_Clippers.AddUnique(c);
        }

        /// <summary>
        /// UnRegister a IClipper element
        /// </summary>
        /// <param name="c">The Element to try and remove.</param>
        public static void Unregister(IClipper c)
        {
            instance.m_Clippers.Remove(c);
        }

        /// <summary>
        /// Disable a IClipper element
        /// </summary>
        /// <param name="c">The Element to try and disable.</param>
        public static void Disable(IClipper c)
        {
            instance.m_Clippers.DisableItem(c);
        }
    }
}
