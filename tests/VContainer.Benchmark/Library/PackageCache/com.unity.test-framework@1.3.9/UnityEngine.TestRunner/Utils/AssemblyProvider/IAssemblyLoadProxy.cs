using System;

namespace UnityEngine.TestTools.Utils
{
    internal interface IAssemblyLoadProxy
    {
        IAssemblyWrapper Load(string assemblyString);
    }
}
