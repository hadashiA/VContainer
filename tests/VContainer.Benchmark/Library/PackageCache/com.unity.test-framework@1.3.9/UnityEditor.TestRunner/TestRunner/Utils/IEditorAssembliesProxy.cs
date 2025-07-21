using System;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IEditorAssembliesProxy
    {
        IAssemblyWrapper[] loadedAssemblies { get; }
    }
}
