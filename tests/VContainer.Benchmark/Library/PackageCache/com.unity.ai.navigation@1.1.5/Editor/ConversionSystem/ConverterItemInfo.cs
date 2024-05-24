#if UNITY_2022_2_OR_NEWER
namespace Unity.AI.Navigation.Editor.Converter
{
    /// <summary>
    /// A structure holding the information for each Item that needs to be Converted.
    /// Descriptor = The ConverterItemDescriptor this item contain.
    /// Index = The index for this item in the list of converter items.
    /// </summary>
    internal struct ConverterItemInfo
    {
        /// <summary> The ConverterItemDescriptor this item contain. </summary>
        public ConverterItemDescriptor descriptor { get; internal set; }

        /// <summary> The index for this item in the list of converter items. </summary>
        public int index { get; internal set; }
    }
}
#endif