using System;
using System.Collections.Generic;
using UnityEditor.Compilation;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal interface IAssemblyNameProvider
  {
    string[] ProjectSupportedExtensions { get; }
    string ProjectGenerationRootNamespace { get; }
    ProjectGenerationFlag ProjectGenerationFlag { get; }

    string GetAssemblyNameFromScriptPath(string path);
    string GetProjectName(string name, string[] defines);
    bool IsInternalizedPackagePath(string path);
    IEnumerable<Assembly> GetAssemblies(Func<string, bool> shouldFileBePartOfSolution);
    IEnumerable<string> GetAllAssetPaths();
    UnityEditor.PackageManager.PackageInfo FindForAssetPath(string assetPath);
    ResponseFileData ParseResponseFile(string responseFilePath, string projectDirectory, string[] systemReferenceDirectories);
    IEnumerable<string> GetRoslynAnalyzerPaths();
    void ToggleProjectGeneration(ProjectGenerationFlag preference);
    void ResetPackageInfoCache();
    void ResetAssembliesCache();
  }
}