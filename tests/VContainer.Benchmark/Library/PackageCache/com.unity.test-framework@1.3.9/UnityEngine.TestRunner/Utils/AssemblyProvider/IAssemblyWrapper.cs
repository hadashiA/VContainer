using System;
using System.Reflection;

namespace UnityEngine.TestTools.Utils
{
    internal interface IAssemblyWrapper
    {
        Assembly Assembly { get; }
        AssemblyName Name { get; }
        string Location { get; }
        AssemblyName[] GetReferencedAssemblies();
    }
}
