using System;
using System.Collections;
using UnityEditor;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Implements <see cref="IEditModeTestYieldInstruction"/>. Creates a yield instruction to enter Play Mode.
    /// </summary>
    public class EnterPlayMode : IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Returns true if the instruction expects a domain reload to occur.
        /// </summary>
        public bool ExpectDomainReload { get; }
        /// <summary>
        /// Returns true if the instruction expects the Unity Editor to be in **Play Mode**.
        /// </summary>
        public bool ExpectedPlaymodeState { get; private set; }
        /// <summary>
        /// When creating an Editor test that uses the UnityTest attribute, use this to trigger the Editor to enter Play Mode.
        /// Throws an exception if the Editor is already in Play Mode or if there is a script compilation error.
        /// </summary>
        /// <param name="expectDomainReload">A flag indication whether to expect a domain reload.</param>
        public EnterPlayMode(bool expectDomainReload = true)
        {
            ExpectDomainReload = expectDomainReload;
        }

        /// <summary>
        /// Performs the multi-step instructions of entering PlayMode.
        /// </summary>
        /// <returns>An IEnumerator with the async steps.</returns>
        /// <exception cref="Exception">An exception is thrown if the editor is already in PlayMode or if script compilation failed.</exception>
        public IEnumerator Perform()
        {
            if (EditorApplication.isPlaying)
            {
                throw new Exception("Editor is already in PlayMode");
            }
            if (EditorUtility.scriptCompilationFailed)
            {
                throw new Exception("Script compilation failed");
            }
            yield return null;
            ExpectedPlaymodeState = true;

            EditorApplication.UnlockReloadAssemblies();
            EditorApplication.isPlaying = true;

            while (!EditorApplication.isPlaying)
            {
                yield return null;
            }
        }
    }
}
