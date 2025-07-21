using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// An interface implemented by a MonoBehaviour test.
    /// </summary>
    public interface IMonoBehaviourTest
    {
        /// <summary>True when the test is considered finished.</summary>
        bool IsTestFinished {get; }
    }
}
