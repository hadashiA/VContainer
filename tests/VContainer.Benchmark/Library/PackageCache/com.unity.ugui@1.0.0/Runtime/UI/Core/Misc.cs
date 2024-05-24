namespace UnityEngine.UI
{
    /// <summary>
    /// Helper class containing generic functions used throughout the UI library.
    /// </summary>

    internal static class Misc
    {
        /// <summary>
        /// Destroy the specified object, immediately if in edit mode.
        /// </summary>

        static public void Destroy(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    if (obj is GameObject)
                    {
                        GameObject go = obj as GameObject;
                        go.transform.parent = null;
                    }

                    Object.Destroy(obj);
                }
                else Object.DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Destroy the specified object immediately, unless not in the editor, in which case the regular Destroy is used instead.
        /// </summary>

        static public void DestroyImmediate(Object obj)
        {
            if (obj != null)
            {
                if (Application.isEditor) Object.DestroyImmediate(obj);
                else Object.Destroy(obj);
            }
        }
    }
}
