using System;
using System.Collections;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// In an Edit Mode test, you can use `IEditModeTestYieldInstruction` interface to implement your own instruction. There are also a couple of commonly used implementations available:
    /// - [EnterPlayMore](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/api/UnityEngine.TestTools.EnterPlayMode.html)
    /// - <see cref = "ExitPlayMode"/> 
    /// - <see cref = "RecompileScripts"/> 
    /// - <see cref = "WaitForDomainReload"/> 
    /// </summary>
    public interface IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Whether or not the instruction expects a domain reload to occur.
        /// </summary>
        bool ExpectDomainReload { get; }
        /// <summary>
        /// Whether or not the instruction expects the Unity Editor to be in **Play Mode**.
        /// </summary>
        bool ExpectedPlaymodeState { get; }
        /// <summary>
        ///  Used to define multi-frame operations performed when instantiating a yield instruction.
        /// </summary>
        /// <returns>Enumerable collection of operations to perform.</returns>
        IEnumerator Perform();
    }
}
