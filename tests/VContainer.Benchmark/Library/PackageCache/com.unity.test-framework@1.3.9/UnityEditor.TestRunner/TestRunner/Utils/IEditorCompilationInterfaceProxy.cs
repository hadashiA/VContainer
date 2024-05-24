using System;
using UnityEditor.Scripting.ScriptCompilation;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IEditorCompilationInterfaceProxy
    {
        ScriptAssembly[] GetAllEditorScriptAssemblies();
        PrecompiledAssembly[] GetAllPrecompiledAssemblies();
    }
}
