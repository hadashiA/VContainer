#if UNITY_2022_2_OR_NEWER
using System;

namespace Unity.AI.Navigation.Editor.Converter
{
    /// <summary>
    /// A structure holding the information for each Item that needs to be Converted.
    /// Name = The Name of the asset that is being converted.
    /// Info = Information that can be used to store some data. This will also be shown in the UI.
    /// WarningMessage = If there are some issues with the converter that we already know about.
    ///     Example: If we know it is a custom shader, we can not convert it so we add the information here.
    /// HelpLink = Link to the documentation of how to convert this asset. Useful if the conversion failed or if we know we can not convert this asset automatically.
    /// </summary>
    [Serializable]
    internal struct ConverterItemDescriptor
    {
        /// <summary> Name of the asset being converted. This will be shown in the UI. </summary>
        public string name;
        /// <summary> Information that can be used to store some data. This will also be shown in the UI. </summary>
        public string info;
        /// <summary> If there are some issues with the converter that we already know about during init phase. This will be added as a tooltip on the warning icon. </summary>
        public string warningMessage;
        /// <summary> Link to the documentation of how to convert this asset. Useful if the conversion failed or if we know we can not convert this asset automatically. </summary>
        public string helpLink;
    }
}
#endif
