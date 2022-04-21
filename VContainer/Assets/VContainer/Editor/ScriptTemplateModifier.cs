using System.IO;
using UnityEditor;
using UnityEngine;
using VContainer.Unity;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.Compilation;
#endif

namespace VContainer.Editor
{
    public sealed class ScriptTemplateProcessor : UnityEditor.AssetModificationProcessor
    {
#if UNITY_2020_2_OR_NEWER
        const string RootNamespaceBeginTag = "#ROOTNAMESPACEBEGIN#";
        const string RootNamespaceEndTag = "#ROOTNAMESPACEEND#";
#endif

        const string MonoInstallerTemplate =
            "using VContainer;\n" +
            "using VContainer.Unity;\n" +
            "\n" +
#if UNITY_2020_2_OR_NEWER
            RootNamespaceBeginTag + "\n" +
#endif
            "public class #SCRIPTNAME# : LifetimeScope\n" +
            "{\n" +
            "    protected override void Configure(IContainerBuilder builder)\n" +
            "    {\n" +
            "    }\n" +
            "}\n" +
#if UNITY_2020_2_OR_NEWER
            RootNamespaceEndTag + "\n" +
#endif
            "";

        public static void OnWillCreateAsset(string metaPath)
        {
            if (VContainerSettings.Instance != null && VContainerSettings.Instance.DisableScriptModifier)
            {
                return;
            }

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
                content = RemoveOrInsertNamespaceSimple(content, rootNamespace);
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

#if UNITY_2020_2_OR_NEWER
        // https://github.com/Unity-Technologies/UnityCsReference/blob/2020.2/Editor/Mono/ProjectWindow/ProjectWindowUtil.cs#L495-L550
        static string RemoveOrInsertNamespaceSimple(string content, string rootNamespace)
        {
            const char eol = '\n';

            if (string.IsNullOrWhiteSpace(rootNamespace))
            {
                return content
                    .Replace(RootNamespaceBeginTag + eol, "")
                    .Replace(RootNamespaceEndTag + eol, "");
            }

            var lines = content.Split(eol);

            var startAt = ArrayUtility.IndexOf(lines, RootNamespaceBeginTag);
            var endAt = ArrayUtility.IndexOf(lines, RootNamespaceEndTag);

            lines[startAt] = $"namespace {rootNamespace}\n{{";
            {
                for (var i = startAt + 1; i < endAt; ++i)
                {
                    lines[i] = $"    {lines[i]}";
                }
            }
            lines[endAt] = "}";

            return string.Join(eol.ToString(), lines);
        }
#endif
    }
}
