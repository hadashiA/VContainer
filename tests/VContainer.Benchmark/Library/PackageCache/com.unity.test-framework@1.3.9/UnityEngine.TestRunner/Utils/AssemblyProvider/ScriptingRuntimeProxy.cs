using System;

namespace UnityEngine.TestTools.Utils
{
    internal class ScriptingRuntimeProxy : IScriptingRuntimeProxy
    {
        public string[] GetAllUserAssemblies()
        {
            return ScriptingRuntime.GetAllUserAssemblies();
        }
    }
}
