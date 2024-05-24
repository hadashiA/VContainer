using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// A flag indicating whether to run Edit Mode or Play Mode tests.
    /// </summary>
    [Flags]
    public enum TestMode
    {
        /// <summary>
        /// Run EditMode tests.
        /// </summary>
        EditMode = 1 << 0,
        /// <summary>
        /// Run PlayMode tests.
        /// </summary>
        PlayMode = 1 << 1
    }
}
