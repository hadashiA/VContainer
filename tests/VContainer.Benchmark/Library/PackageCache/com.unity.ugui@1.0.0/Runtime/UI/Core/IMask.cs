using System;

namespace UnityEngine.UI
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Not supported anymore.", true)]
    public interface IMask
    {
        bool Enabled();
        RectTransform rectTransform { get; }
    }
}
