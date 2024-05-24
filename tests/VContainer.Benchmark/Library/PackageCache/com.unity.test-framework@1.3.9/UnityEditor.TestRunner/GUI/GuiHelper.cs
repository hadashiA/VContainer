using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.CodeEditor;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class GuiHelper : IGuiHelper
    {
        public GuiHelper(IMonoCecilHelper monoCecilHelper, IAssetsDatabaseHelper assetsDatabaseHelper)
        {
            MonoCecilHelper = monoCecilHelper;
            AssetsDatabaseHelper = assetsDatabaseHelper;
            GetCSFiles = (dirPath, fileExtension) =>
            {
                return Directory.GetFiles(dirPath, $"*{fileExtension}", SearchOption.AllDirectories)
                    .Select(Paths.UnifyDirectorySeparator);
            };
        }

        internal Func<string, string, IEnumerable<string>> GetCSFiles;
        protected IMonoCecilHelper MonoCecilHelper { get; private set; }
        public IAssetsDatabaseHelper AssetsDatabaseHelper { get; private set; }
        public IExternalCodeEditor Editor { get; internal set; }
        private const string FileExtension = ".cs";

        public void OpenScriptInExternalEditor(Type type, MethodInfo method)
        {
            var fileOpenInfo = GetFileOpenInfo(type, method);

            if (string.IsNullOrEmpty(fileOpenInfo.FilePath))
            {
                Debug.LogWarning("Failed to open test method source code in external editor. Inconsistent filename and yield return operator in target method.");

                return;
            }

            if (fileOpenInfo.LineNumber == 1)
            {
                Debug.LogWarning("Failed to get a line number for unity test method. So please find it in opened file in external editor.");
            }

            if (!fileOpenInfo.FilePath.Contains("Assets"))
            {
                (Editor ?? CodeEditor.CurrentEditor).OpenProject(fileOpenInfo.FilePath, fileOpenInfo.LineNumber, 1);
            }
            else
            {
                AssetsDatabaseHelper.OpenAssetInItsDefaultExternalEditor(fileOpenInfo.FilePath, fileOpenInfo.LineNumber);
            }
        }

        public IFileOpenInfo GetFileOpenInfo(Type type, MethodInfo method)
        {
            var fileOpenInfo = MonoCecilHelper.TryGetCecilFileOpenInfo(type, method);
            if (string.IsNullOrEmpty(fileOpenInfo.FilePath))
            {
                var dirPath = Paths.UnifyDirectorySeparator(Application.dataPath);
                var allCsFiles = GetCSFiles(dirPath, FileExtension);

                var fileName = allCsFiles.FirstOrDefault(x =>
                    x.Split(Path.DirectorySeparatorChar).Last().Equals(string.Concat(GetTestFileName(type), FileExtension)));

                fileOpenInfo.FilePath = fileName ?? string.Empty;
            }

            if (!fileOpenInfo.FilePath.Contains("Assets"))
            {
                return fileOpenInfo;
            }
            fileOpenInfo.FilePath = FilePathToAssetsRelativeAndUnified(fileOpenInfo.FilePath);

            return fileOpenInfo;
        }

        internal static string GetTestFileName(Type type)
        {
            //This handles the case of a test in a nested class, getting the name of the base class
            if (type.FullName != null && type.Namespace!=null && type.FullName.Contains("+"))
            {
                var removedNamespace = type.FullName.Substring(type.Namespace.Length+1);
                return removedNamespace.Substring(0,removedNamespace.IndexOf("+", StringComparison.Ordinal));
            }
            return type.Name;
        }

        public string FilePathToAssetsRelativeAndUnified(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            filePath = Paths.UnifyDirectorySeparator(filePath);

            const string assetsFolder = "Assets";
            var assetsFolderIndex = filePath.IndexOf(assetsFolder);
            if (assetsFolderIndex < 0)
                return filePath;

            return filePath.Substring(assetsFolderIndex, filePath.Length - assetsFolderIndex);
        }

        public bool OpenScriptInExternalEditor(string stacktrace)
        {
            if (string.IsNullOrEmpty(stacktrace))
                return false;

            var regex = new Regex("in (?<path>.*):{1}(?<line>[0-9]+)");

            var matchingLines = stacktrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(x => regex.IsMatch(x)).ToList();
            if (!matchingLines.Any())
                return false;

            var fileOpenInfos = matchingLines
                .Select(x => regex.Match(x))
                .Select(x =>
                    new FileOpenInfo
                    {
                        FilePath = x.Groups["path"].Value,
                        LineNumber = int.Parse(x.Groups["line"].Value)
                    }).ToList();

            var fileOpenInfo = fileOpenInfos
                .FirstOrDefault(openInfo => !string.IsNullOrEmpty(openInfo.FilePath) && File.Exists(openInfo.FilePath));

            if (fileOpenInfo == null)
            {
                return false;
            }

            var filePath = FilePathToAssetsRelativeAndUnified(fileOpenInfo.FilePath);
            AssetsDatabaseHelper.OpenAssetInItsDefaultExternalEditor(filePath, fileOpenInfo.LineNumber);

            return true;
        }
    }
}
