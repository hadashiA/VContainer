using System;
using System.Collections;
using UnityEditor;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Implements <see cref="IEditModeTestYieldInstruction"/>. A new instance of the class is a yield instruction to exit Play Mode.
    /// </summary>
    public class ExitPlayMode : IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Gets the value of ExpectDomainReload
        /// </summary>
        public bool ExpectDomainReload { get; }
        /// <summary>
        /// Gets the value of ExpectedPlaymodeState
        /// </summary>
        public bool ExpectedPlaymodeState { get; private set; }
        /// <summary>
        /// Sets ExpectDomainReload and ExpectedPlaymodeState to false.
        /// </summary>
        public ExitPlayMode()
        {
            ExpectDomainReload = false;
            ExpectedPlaymodeState = false;
        }

        /// <summary>
        /// Performs the multi-step instruction of exiting PlayMode.
        /// </summary>
        /// <returns>An IEnumerator with the async steps.</returns>
        /// <exception cref="Exception">An exception is thrown if the editor is not in PlayMode.</exception>
        public IEnumerator Perform()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new Exception("Editor is already in EditMode");
            }

            EditorApplication.isPlaying = false;
            while (EditorApplication.isPlaying)
            {
                yield return null;
            }
        }
    }
}
