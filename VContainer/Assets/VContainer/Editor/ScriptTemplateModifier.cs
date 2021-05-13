using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using System.Reflection;
using UnityEditor.Compilation;
#endif

namespace VContainer.Editor
{
    public sealed class ScriptTemplateProcessor : UnityEditor.AssetModificationProcessor
    {
        const string MonoInstallerTemplate =
            "using VContainer;\n" +
            "using VContainer.Unity;\n" +
            "\n" +
#if UNITY_2020_2_OR_NEWER
            "    #ROOTNAMESPACEBEGIN#\n" +
#endif
            "public class #SCRIPTNAME# : LifetimeScope\n" +
            "{\n" +
            "    protected override void Configure(IContainerBuilder builder)\n" +
            "    {\n" +
            "    }\n" +
            "}\n" +
#if UNITY_2020_2_OR_NEWER
            "#ROOTNAMESPACEEND#\n" +
#endif
            "";

#if UNITY_2020_2_OR_NEWER
        static MethodInfo _RemoveOrInsertNamespace = null;
        // https://github.com/Unity-Technologies/UnityCsReference/blob/2020.2/Editor/Mono/ProjectWindow/ProjectWindowUtil.cs#L495-L550
        static string RemoveOrInsertNamespace(string content, string rootNamespace)
        {
            if (_RemoveOrInsertNamespace == null)
            {
                _RemoveOrInsertNamespace = typeof(ProjectWindowUtil).GetMethod("RemoveOrInsertNamespace", BindingFlags.Static | BindingFlags.NonPublic);
            }

            return (string)_RemoveOrInsertNamespace.Invoke(null, new object[]{ content, rootNamespace });
        }
#endif

        public static void OnWillCreateAsset(string metaPath)
        {
            var suffixIndex = metaPath.LastIndexOf(".meta");
            if (suffixIndex < 0)
            {
                return;
            }

            var scriptPath = metaPath.Substring(0, suffixIndex);
            var basename = Path.GetFileNameWithoutExtension(scriptPath);
            var extname = Path.GetExtension(scriptPath);
            if (extname != ".cs")
            {
                return;
            }

            if (!scriptPath.EndsWith("LifetimeScope.cs"))
            {
                return;
            }

            var content = MonoInstallerTemplate.Replace("#SCRIPTNAME#", basename);

#if UNITY_2020_2_OR_NEWER
            {
                var rootNamespace = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(scriptPath);
                content = RemoveOrInsertNamespace(content, rootNamespace);
            }
#endif

            if (scriptPath.StartsWith("Assets/"))
            {
                scriptPath = scriptPath.Substring("Assets/".Length);
            }

            var fullPath = Path.Combine(Application.dataPath, scriptPath);
            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
        }
    }
}
