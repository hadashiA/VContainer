using System.IO;
using UnityEditor;
using UnityEngine;

namespace VContainer.Editor
{
    public sealed class ScriptTemplateProcessor : UnityEditor.AssetModificationProcessor
    {
        const string MonoInstallerTemplate =
            "using VContainer;\n" +
            "using VContainer.Unity;\n" +
            "\n" +
            "public sealed class #SCRIPTNAME# : MonoInstaller\n" +
            "{\n" +
            "    public override void Install(IContainerBuilder builder)\n" +
            "    {\n" +
            "    }\n" +
            "}\n";

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

            if (scriptPath.StartsWith("Assets/"))
            {
                scriptPath = scriptPath.Substring("Assets/".Length);
            }

            var fullPath = Path.Combine(Application.dataPath, scriptPath);
            if (scriptPath.EndsWith("Installer.cs"))
            {
                var content = MonoInstallerTemplate.Replace("#SCRIPTNAME#", basename);
                File.WriteAllText(fullPath, content);
                AssetDatabase.Refresh();
            }
        }
    }
}
