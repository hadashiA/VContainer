using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityEngine.UI.Tests
{
    // Hook into the graphic callback so we can do our check.
    public class ImageHook : Image
    {
        public bool isGeometryUpdated;
        public bool isLayoutRebuild;
        public bool isMaterialRebuilt;
        public Rect cachedClipRect;

        public void ResetTest()
        {
            isGeometryUpdated = false;
            isLayoutRebuild = false;
            isMaterialRebuilt = false;
        }

        public override void SetLayoutDirty()
        {
            base.SetLayoutDirty();
            isLayoutRebuild = true;
        }

        public override void SetMaterialDirty()
        {
            base.SetMaterialDirty();
            isMaterialRebuilt = true;
        }

        protected override void UpdateGeometry()
        {
            base.UpdateGeometry();
            isGeometryUpdated = true;
        }

        public override void SetClipRect(Rect clipRect, bool validRect)
        {
            cachedClipRect = clipRect;
            if (validRect)
                canvasRenderer.EnableRectClipping(clipRect);
            else
                canvasRenderer.DisableRectClipping();
        }
    }
}
