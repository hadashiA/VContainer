using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// WaitForDomainReload is an <see cref="IEditModeTestYieldInstruction"/> that you can yield in Edit Mode tests. It delays the execution of scripts until after an incoming domain reload. If the domain reload results in a script compilation failure, then it throws an exception.
    /// </summary>
    public class WaitForDomainReload : IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Create a new instance of the `WaitForDomainReload` yield instruction.
        /// <example>
        /// <code>
        /// [UnitySetUp]
        /// public IEnumerator SetUp()
        /// {
        ///     File.Copy("Resources/MyDll.dll", @"Assets/MyDll.dll", true); // Trigger a domain reload.
        ///     AssetDatabase.Refresh();
        ///     yield return new WaitForDomainReload();
        /// }
        /// </code>
        /// </example>
        /// </summary>
        public WaitForDomainReload()
        {
            ExpectDomainReload = true;
        }

        /// <summary>
        /// Returns true if the instruction expects a domain reload to occur.
        /// </summary>
        public bool ExpectDomainReload { get; }
        /// <summary>
        /// Returns true if the instruction expects the Unity Editor to be in **Play Mode**.
        /// </summary>
        public bool ExpectedPlaymodeState { get; }

        /// <summary>
        /// Perform the multi step action of waiting for a domain reload.
        /// </summary>
        /// <returns>An IEnumerator with steps.</returns>
        /// <exception cref="Exception">Throws an exception if script compilation failed or if the expected domain reload did not occur.</exception>
        public IEnumerator Perform()
        {
            EditorApplication.UnlockReloadAssemblies();

            while (InternalEditorUtility.IsScriptReloadRequested() || EditorApplication.isCompiling)
            {
                yield return null;
            }

            // Add this point the domain reload should have occured and stopped any further progress on the instruction.
            EditorApplication.LockReloadAssemblies();
            throw new Exception(
                EditorUtility.scriptCompilationFailed ? 
                    "Script compilation failed" : 
                    "Expected domain reload, but it did not occur");
        }
    }
}
