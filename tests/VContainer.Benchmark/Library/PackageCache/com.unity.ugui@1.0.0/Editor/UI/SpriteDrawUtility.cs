using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI
{
    /// <summary>
    /// PropertyDrawer for [[SpriteState]].
    /// This is a PropertyDrawer for SpriteState it is implemented using the standard unity PropertyDrawer framework.
    /// </summary>
    internal class SpriteDrawUtility
    {
        static Texture2D s_ContrastTex;

        // Returns a usable texture that looks like a high-contrast checker board.
        static Texture2D contrastTexture
        {
            get
            {
                if (s_ContrastTex == null)
                    s_ContrastTex = CreateCheckerTex(
                        new Color(0f, 0.0f, 0f, 0.5f),
                        new Color(1f, 1f, 1f, 0.5f));
                return s_ContrastTex;
            }
        }

        // Create a checker-background texture.
        static Texture2D CreateCheckerTex(Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
            for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
            for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
            for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        // Create a gradient texture.
        static Texture2D CreateGradientTex()
        {
            Texture2D tex = new Texture2D(1, 16);
            tex.name = "[Generated] Gradient Texture";
            tex.hideFlags = HideFlags.DontSave;

            Color c0 = new Color(1f, 1f, 1f, 0f);
            Color c1 = new Color(1f, 1f, 1f, 0.4f);

            for (int i = 0; i < 16; ++i)
            {
                float f = Mathf.Abs((i / 15f) * 2f - 1f);
                f *= f;
                tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        // Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
        static void DrawTiledTexture(Rect rect, Texture tex)
        {
            float u = rect.width / tex.width;
            float v = rect.height / tex.height;

            Rect texCoords = new Rect(0, 0, u, v);
            TextureWrapMode originalMode = tex.wrapMode;
            tex.wrapMode = TextureWrapMode.Repeat;
            GUI.DrawTextureWithTexCoords(rect, tex, texCoords);
            tex.wrapMode = originalMode;
        }

        // Draw the specified Image.
        public static void DrawSprite(Sprite sprite, Rect drawArea, Color color)
        {
            if (sprite == null)
                return;

            Texture2D tex = sprite.texture;
            if (tex == null)
                return;

            Rect outer = sprite.rect;
            Rect inner = outer;
            inner.xMin += sprite.border.x;
            inner.yMin += sprite.border.y;
            inner.xMax -= sprite.border.z;
            inner.yMax -= sprite.border.w;

            Vector4 uv4 = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            Rect uv = new Rect(uv4.x, uv4.y, uv4.z - uv4.x, uv4.w - uv4.y);
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
            padding.x /= outer.width;
            padding.y /= outer.height;
            padding.z /= outer.width;
            padding.w /= outer.height;

            DrawSprite(tex, drawArea, padding, outer, inner, uv, color, null);
        }

        // Draw the specified Image.
        public static void DrawSprite(Texture tex, Rect drawArea, Rect outer, Rect uv, Color color)
        {
            DrawSprite(tex, drawArea, Vector4.zero, outer, outer, uv, color, null);
        }

        // Draw the specified Image.
        private static void DrawSprite(Texture tex, Rect drawArea, Vector4 padding, Rect outer, Rect inner, Rect uv, Color color, Material mat)
        {
            // Create the texture rectangle that is centered inside rect.
            Rect outerRect = drawArea;
            outerRect.width = Mathf.Abs(outer.width);
            outerRect.height = Mathf.Abs(outer.height);

            if (outerRect.width > 0f)
            {
                float f = drawArea.width / outerRect.width;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (drawArea.height > outerRect.height)
            {
                outerRect.y += (drawArea.height - outerRect.height) * 0.5f;
            }
            else if (outerRect.height > drawArea.height)
            {
                float f = drawArea.height / outerRect.height;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (drawArea.width > outerRect.width)
                outerRect.x += (drawArea.width - outerRect.width) * 0.5f;

            // Draw the background
            EditorGUI.DrawTextureTransparent(outerRect, null, ScaleMode.ScaleToFit, outer.width / outer.height);

            // Draw the Image
            GUI.color = color;

            Rect paddedTexArea = new Rect(
                outerRect.x + outerRect.width * padding.x,
                outerRect.y + outerRect.height * padding.w,
                outerRect.width - (outerRect.width * (padding.z + padding.x)),
                outerRect.height - (outerRect.height * (padding.w + padding.y))
            );

            if (mat == null)
            {
                GUI.DrawTextureWithTexCoords(paddedTexArea, tex, uv, true);
            }
            else
            {
                // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
                // using BeginGroup/EndGroup, and there is no way to specify a UV rect...
                EditorGUI.DrawPreviewTexture(paddedTexArea, tex, mat);
            }

            // Draw the border indicator lines
            GUI.BeginGroup(outerRect);
            {
                tex = contrastTexture;
                GUI.color = Color.white;

                if (inner.xMin != outer.xMin)
                {
                    float x = (inner.xMin - outer.xMin) / outer.width * outerRect.width - 1;
                    DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
                }

                if (inner.xMax != outer.xMax)
                {
                    float x = (inner.xMax - outer.xMin) / outer.width * outerRect.width - 1;
                    DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
                }

                if (inner.yMin != outer.yMin)
                {
                    // GUI.DrawTexture is top-left based rather than bottom-left
                    float y = (inner.yMin - outer.yMin) / outer.height * outerRect.height - 1;
                    DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
                }

                if (inner.yMax != outer.yMax)
                {
                    float y = (inner.yMax - outer.yMin) / outer.height * outerRect.height - 1;
                    DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
                }
            }

            GUI.EndGroup();
        }
    }
}
