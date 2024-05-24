using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// A flag indicating the targeted test platforms.
    /// </summary>
    [Flags]
    [Serializable]
    public enum TestPlatform : byte
    {
        /// <summary>
        /// Both platforms.
        /// </summary>
        All = 0xFF,
        /// <summary>
        /// The EditMode test platform.
        /// </summary>
        EditMode = 1 << 1,
        /// <summary>
        /// The PlayMode test platform.
        /// </summary>
        PlayMode = 1 << 2
    }

    internal static class TestPlatformEnumExtensions
    {
        public static bool IsFlagIncluded(this TestPlatform flags, TestPlatform flag)
        {
            return (flags & flag) == flag;
        }

        public static TestPlatform MergeFlags(this TestPlatform[] flags)
        {
            TestPlatform mergedFlag = default;
            foreach (var flag in flags)
            {
                mergedFlag |= flag;
            }

            return mergedFlag;
        }
    }
}
