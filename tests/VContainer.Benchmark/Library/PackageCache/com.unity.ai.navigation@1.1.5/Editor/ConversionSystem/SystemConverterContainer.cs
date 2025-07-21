#if UNITY_2022_2_OR_NEWER
namespace Unity.AI.Navigation.Editor.Converter
{
    /// <summary>
    /// A class to contain converters. This is for a common set of converters.
    /// For example: Converters that is for Built-in to URP would have it's own container.
    /// </summary>
    public abstract class SystemConverterContainer
    {
        /// <summary>
        /// The name of the Container. This will show up int the UI.
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// The information for this container.
        /// This will be shown in the UI to tell the user some information about the converters that this container are targeting.
        /// </summary>
        public abstract string info { get; }
    }
}
#endif
