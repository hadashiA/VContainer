#if UNITY_2022_2_OR_NEWER
using System;

namespace Unity.AI.Navigation.Editor.Converter
{
    internal abstract class SystemConverter
    {
        /// <summary>
        /// Name of the converter.
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// The information when hovering over the converter.
        /// </summary>
        public abstract string info { get; }

        /// <summary>
        /// A check if the converter is enabled or not. Can be used to do a check if prerequisites are met to have it enabled or disabled.
        /// </summary>
        public virtual bool isEnabled => true;

        /// <summary>
        /// A priority of the converter. The lower the number (can be negative), the earlier it will be executed. Can be used to make sure that a converter runs before another converter.
        /// </summary>
        public virtual int priority => 0;

        /// <summary>
        /// A check to see if the converter needs to create the index.
        /// This will only need to be set to true if the converter is using search api, and search queries.
        /// If set to true the converter framework will create the indexer and remove it after all search queries are done.
        /// </summary>
        public virtual bool needsIndexing => false;

        /// <summary>
        /// This method getting triggered when clicking the listview item in the UI.
        /// </summary>
        public virtual void OnClicked(int index)
        {
        }

        // This is so that we can have different segment in our UI, example Unity converters, your custom converters etc..
        // This is not implemented yet
        public virtual string category { get; }

        // This is in which drop down item the converter belongs to.
        // Not properly implemented yet
        public abstract Type container { get; }

        /// <summary>
        /// This runs when initializing the converter. To gather data for the UI and also for the converter if needed.
        /// </summary>
        /// <param name="context">The context that will be used to initialize data for the converter.</param>
        public abstract void OnInitialize(InitializeConverterContext context, Action callback);

        /// <summary>
        /// The method that will be run before Run method if needed.
        /// </summary>
        public virtual void OnPreRun()
        {
        }

        /// <summary>
        /// The method that will be run when converting the assets.
        /// </summary>
        /// <param name="context">The context that will be used when executing converter.</param>
        public abstract void OnRun(ref RunItemContext context);

        /// <summary>
        /// The method that will be run after the converters are done if needed.
        /// </summary>
        public virtual void OnPostRun()
        {
        }
    }
}
#endif
