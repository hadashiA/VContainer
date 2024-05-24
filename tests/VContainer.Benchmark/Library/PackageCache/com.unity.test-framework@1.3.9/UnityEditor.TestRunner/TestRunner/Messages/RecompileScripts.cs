using System;
using System.Collections;
using UnityEditor;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// `RecompileScripts` is an <see cref="IEditModeTestYieldInstruction"/> that you can yield in Edit Mode tests. It lets you trigger a recompilation of scripts in the Unity Editor.
    /// </summary>
    public class RecompileScripts : IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Creates a new instance of the `RecompileScripts` yield instruction.
        /// <example>
        /// <code>
        /// [UnitySetUp]
        /// public IEnumerator SetUp()
        /// {
        ///     using (var file = File.CreateText("Assets/temp/myScript.cs"))
        ///     {
        ///         file.Write("public class ATempClass {  }");
        ///     }
        ///     AssetDatabase.Refresh();
        ///     yield return new RecompileScripts();
        /// }
        /// </code>
        /// </example>
        /// </summary>
        public RecompileScripts() : this(true)
        {
        }
        /// <summary>
        /// Creates a new instance of the `RecompileScripts` yield instruction.
        /// </summary>
        /// <param name="expectScriptCompilation">This parameter indicates if you expect a script compilation to start (defaults to true). If a script compilation does not start and `expectScriptCompilation` is true, then it throws an exception.</param>
        public RecompileScripts(bool expectScriptCompilation) : this(expectScriptCompilation, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the `RecompileScripts` yield instruction.
        /// </summary>
        /// <param name="expectScriptCompilation">This parameter indicates if you expect a script compilation to start (defaults to true).  If a script compilation does not start and `expectScriptCompilation` is `true`, then it throws an exception.</param>
        /// <param name="expectScriptCompilationSuccess">This parameter indicates if you expect a script compilation to succeed. If not succeeded then an exception will be thrown.</param>
        public RecompileScripts(bool expectScriptCompilation, bool expectScriptCompilationSuccess)
        {
            ExpectScriptCompilation = expectScriptCompilation;
            ExpectScriptCompilationSuccess = expectScriptCompilationSuccess;
            ExpectDomainReload = true;
        }

        /// <summary>
        /// Returns true if the instruction expects a domain reload to occur.
        /// </summary>
        public bool ExpectDomainReload { get; private set; }
        /// <summary>
        /// Returns true if the instruction expects the Unity Editor to be in **Play Mode**.
        /// </summary>
        public bool ExpectedPlaymodeState { get; }
        /// <summary>
        /// Indicates whether a script compilation is expected.
        /// </summary>
        public bool ExpectScriptCompilation { get; private set; }
        /// <summary>
        /// Indicates whether the expected script compilation is expected to succeed.
        /// </summary>
        public bool ExpectScriptCompilationSuccess { get; private set; }

        /// <summary>
        /// The current active instance of the RecompileScripts yield instruction.
        /// </summary>
        public static RecompileScripts Current { get; private set; }

        /// <summary>
        /// Perform the multi step instruction of triggering a recompilation of scripts and waiting for its completion.
        /// </summary>
        /// <returns>An IEnumerator with the async steps.</returns>
        /// <exception cref="Exception">Throws an exception if the editor does not need to recompile scripts or if the script compilation failed when expected to succeed.</exception>
        public IEnumerator Perform()
        {
            Current = this;

            // We need to yield, to give the test runner a chance to prepare for the domain reload
            //  If the script compilation happens very fast, then EditModeRunner.MoveNextAndUpdateYieldObject will not have a chance to set m_CurrentYieldObject
            // This really should be fixed in EditModeRunner.MoveNextAndUpdateYieldObject
            yield return null;

            AssetDatabase.Refresh();

            if (ExpectScriptCompilation && !EditorApplication.isCompiling)
            {
                Current = null;
                throw new Exception("Editor does not need to recompile scripts");
            }

            EditorApplication.UnlockReloadAssemblies();

            while (EditorApplication.isCompiling)
            {
                yield return null;
            }

            Current = null;

            if (ExpectScriptCompilationSuccess && EditorUtility.scriptCompilationFailed)
            {
                EditorApplication.LockReloadAssemblies();
                throw new Exception("Script compilation failed");
            }
        }
    }
}
