using System;
using System.IO;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class TestListGUIHelper
    {
        private const string kResourcesTemplatePath = "Resources/ScriptTemplates";
        private const string kAssemblyDefinitionTestTemplate = "92-Assembly Definition-NewTestAssembly.asmdef.txt";

        private const string kAssemblyDefinitionEditModeTestTemplate =
            "92-Assembly Definition-NewEditModeTestAssembly.asmdef.txt";

        private const string kTestScriptTemplate = "83-C# Script-NewTestScript.cs.txt";
        private const string kNewTestScriptName = "NewTestScript.cs";
        private const string kNunit = "nunit.framework.dll";

        [MenuItem("Assets/Create/Testing/Tests Assembly Folder", false, 83)]
        public static void MenuItemAddFolderAndAsmDefForTesting()
        {
            AddFolderAndAsmDefForTesting();
        }

        [MenuItem("Assets/Create/Testing/Tests Assembly Folder", true, 83)]
        public static bool MenuItemAddFolderAndAsmDefForTestingWithValidation()
        {
            return !SelectedFolderContainsTestAssembly();
        }

        public static void AddFolderAndAsmDefForTesting(bool isEditorOnly = false)
        {
            ProjectWindowUtil.CreateFolderWithTemplates("Tests",
                isEditorOnly ? kAssemblyDefinitionEditModeTestTemplate : kAssemblyDefinitionTestTemplate);
        }

        public static bool SelectedFolderContainsTestAssembly()
        {
            var theNearestCustomScriptAssembly = GetTheNearestCustomScriptAssembly();
            if (theNearestCustomScriptAssembly != null)
            {
                return theNearestCustomScriptAssembly.PrecompiledReferences != null && theNearestCustomScriptAssembly.PrecompiledReferences.Any(x => Path.GetFileName(x) == kNunit);
            }

            return false;
        }

        [MenuItem("Assets/Create/Testing/C# Test Script", false, 83)]
        public static void AddTest()
        {
            var basePath = Path.Combine(EditorApplication.applicationContentsPath, kResourcesTemplatePath);
            var destPath = Path.Combine(GetActiveFolderPath(), kNewTestScriptName);
            var templatePath = Path.Combine(basePath, kTestScriptTemplate);
            var icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<DoCreateScriptAsset>(), destPath, icon, templatePath);

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/Testing/C# Test Script", true, 83)]
        public static bool CanAddScriptAndItWillCompile()
        {
            return CanAddEditModeTestScriptAndItWillCompile() || CanAddPlayModeTestScriptAndItWillCompile();
        }

        public static bool CanAddEditModeTestScriptAndItWillCompile()
        {
            var theNearestCustomScriptAssembly = GetTheNearestCustomScriptAssembly();
            if (theNearestCustomScriptAssembly != null)
            {
                return (theNearestCustomScriptAssembly.AssemblyFlags & AssemblyFlags.EditorOnly) ==
                    AssemblyFlags.EditorOnly;
            }

            var activeFolderPath = GetActiveFolderPath();
            return activeFolderPath.ToLower().Contains("/editor");
        }

        public static bool CanAddPlayModeTestScriptAndItWillCompile()
        {
            if (PlayerSettings.playModeTestRunnerEnabled)
            {
                return true;
            }

            var theNearestCustomScriptAssembly = GetTheNearestCustomScriptAssembly();

            if (theNearestCustomScriptAssembly == null)
            {
                return false;
            }

            var hasTestAssemblyFlag = theNearestCustomScriptAssembly.PrecompiledReferences != null && theNearestCustomScriptAssembly.PrecompiledReferences.Any(x => Path.GetFileName(x) == kNunit);;
            var editorOnlyAssembly = (theNearestCustomScriptAssembly.AssemblyFlags & AssemblyFlags.EditorOnly) != 0;

            return hasTestAssemblyFlag && !editorOnlyAssembly;
        }

        public static string GetActiveFolderPath()
        {
            var path = "Assets";

            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        private static CustomScriptAssembly GetTheNearestCustomScriptAssembly()
        {
            CustomScriptAssembly findCustomScriptAssemblyFromScriptPath;
            try
            {
                findCustomScriptAssemblyFromScriptPath =
                    EditorCompilationInterface.Instance.FindCustomScriptAssemblyFromScriptPath(
                        Path.Combine(GetActiveFolderPath(), "Foo.cs"));
            }
            catch (Exception)
            {
                return null;
            }
            return findCustomScriptAssemblyFromScriptPath;
        }
    }
}
