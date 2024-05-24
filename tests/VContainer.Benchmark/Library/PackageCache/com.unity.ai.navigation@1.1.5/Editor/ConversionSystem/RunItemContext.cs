#if UNITY_2022_2_OR_NEWER
namespace Unity.AI.Navigation.Editor.Converter
{
    /// <summary>
    /// A structure needed for the conversion part of the converter.
    /// This holds the item that is being converted.
    /// </summary>
    internal struct RunItemContext
    {
        ConverterItemInfo m_Item;
        /// <summary> The item that will go through the conversion code. </summary>
        public ConverterItemInfo item => m_Item;

        /// <summary> A bool to set if this item failed to convert. </summary>
        public bool didFail { get; set; }

        /// <summary> Info to store data to be shown in the UI. </summary>
        public string info { get; set; }

        internal bool hasConverted { get; set; }

        /// <summary> Constructor for the RunItemContext. </summary>
        public RunItemContext(ConverterItemInfo item)
        {
            m_Item = item;
            didFail = false;
            info = "";
            hasConverted = false;
        }
    }
}
#endif
