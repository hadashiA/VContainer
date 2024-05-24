using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class AssemblyNameProvider : IAssemblyNameProvider
  {
    private readonly Dictionary<string, UnityEditor.PackageManager.PackageInfo> m_PackageInfoCache = new Dictionary<string, UnityEditor.PackageManager.PackageInfo>();

    ProjectGenerationFlag m_ProjectGenerationFlag = (ProjectGenerationFlag)EditorPrefs.GetInt("unity_project_generation_flag", 3);

    public string[] ProjectSupportedExtensions => EditorSettings.projectGenerationUserExtensions;
    
    public string ProjectGenerationRootNamespace => EditorSettings.projectGenerationRootNamespace;

    private Assembly[] m_AllEditorAssemblies;
    
    private Assembly[] m_AllPlayerAssemblies;

    public ProjectGenerationFlag ProjectGenerationFlag
    {
      get => m_ProjectGenerationFlag;
      private set
      {
        EditorPrefs.SetInt("unity_project_generation_flag", (int)value);
        m_ProjectGenerationFlag = value;
      }
    }

    public string GetAssemblyNameFromScriptPath(string path)
    {
      return CompilationPipeline.GetAssemblyNameFromScriptPath(path);
    }

    public IEnumerable<Assembly> GetAssemblies(Func<string, bool> shouldFileBePartOfSolution)
    {
      if (m_AllEditorAssemblies == null)
        m_AllEditorAssemblies = GetAssembliesByType(AssembliesType.Editor).ToArray();

      if (ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.PlayerAssemblies))
      {
        if (m_AllPlayerAssemblies == null)
          m_AllPlayerAssemblies = GetAssembliesByType(AssembliesType.Player).ToArray();
      }
      
      if (!ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.PlayerAssemblies))
        return m_AllEditorAssemblies.Where(a => a.sourceFiles.Any(shouldFileBePartOfSolution));
      
      return m_AllEditorAssemblies.Concat(m_AllPlayerAssemblies).Where(a => a.sourceFiles.Any(shouldFileBePartOfSolution));
    }

    private static IEnumerable<Assembly> GetAssembliesByType(AssembliesType type)
    {
      foreach (var assembly in CompilationPipeline.GetAssemblies(type))
      {
        var outputPath = type == AssembliesType.Editor ? $@"Temp\Bin\Debug\{assembly.name}\" : $@"Temp\Bin\Debug\{assembly.name}\Player\";
        yield return new Assembly(assembly.name, outputPath, assembly.sourceFiles, assembly.defines,
          assembly.assemblyReferences, assembly.compiledAssemblyReferences, assembly.flags, assembly.compilerOptions
#if UNITY_2020_2_OR_NEWER
          , assembly.rootNamespace
#endif
        );
      }
    }

    public string GetProjectName(string name, string[] defines)
    {
      if (!ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.PlayerAssemblies))
        return name;
      return !defines.Contains("UNITY_EDITOR") ? name + ".Player" : name;
    }

    public IEnumerable<string> GetAllAssetPaths()
    {
      return AssetDatabase.GetAllAssetPaths();
    }

    private static string ResolvePotentialParentPackageAssetPath(string assetPath)
    {
      const string packagesPrefix = "packages/";
      if (!assetPath.StartsWith(packagesPrefix, StringComparison.OrdinalIgnoreCase))
      {
        return null;
      }

      var followupSeparator = assetPath.IndexOf('/', packagesPrefix.Length);
      if (followupSeparator == -1)
      {
        return assetPath.ToLowerInvariant();
      }

      return assetPath.Substring(0, followupSeparator).ToLowerInvariant();
    }

    public UnityEditor.PackageManager.PackageInfo FindForAssetPath(string assetPath)
    {
      var parentPackageAssetPath = ResolvePotentialParentPackageAssetPath(assetPath);
      if (parentPackageAssetPath == null)
      {
        return null;
      }

      if (m_PackageInfoCache.TryGetValue(parentPackageAssetPath, out var cachedPackageInfo))
      {
        return cachedPackageInfo;
      }

      var result = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(parentPackageAssetPath);
      m_PackageInfoCache[parentPackageAssetPath] = result;
      return result;
    }

    public void ResetPackageInfoCache()
    {
      m_PackageInfoCache.Clear();
    }

    public void ResetAssembliesCache()
    {
      m_AllEditorAssemblies = null;
      m_AllPlayerAssemblies = null;
    }

    public bool IsInternalizedPackagePath(string path)
    {
      if (string.IsNullOrEmpty(path.Trim()))
      {
        return false;
      }

      var packageInfo = FindForAssetPath(path);
      if (packageInfo == null)
      {
        return false;
      }

      var packageSource = packageInfo.source;
      switch (packageSource)
      {
        case PackageSource.Embedded:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Embedded);
        case PackageSource.Registry:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Registry);
        case PackageSource.BuiltIn:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.BuiltIn);
        case PackageSource.Unknown:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Unknown);
        case PackageSource.Local:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Local);
        case PackageSource.Git:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Git);
#if UNITY_2019_3_OR_NEWER
        case PackageSource.LocalTarball:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.LocalTarBall);
#endif
      }

      return false;
    }

    public ResponseFileData ParseResponseFile(string responseFilePath, string projectDirectory, string[] systemReferenceDirectories)
    {
      return CompilationPipeline.ParseResponseFile(
        responseFilePath,
        projectDirectory,
        systemReferenceDirectories
      );
    }

    public IEnumerable<string> GetRoslynAnalyzerPaths()
    {
      return PluginImporter.GetAllImporters()
        .Where(i => !i.isNativePlugin && AssetDatabase.GetLabels(i).SingleOrDefault(l => l == "RoslynAnalyzer") != null)
        .Select(i => i.assetPath);
    }

    public void ToggleProjectGeneration(ProjectGenerationFlag preference)
    {
      if (ProjectGenerationFlag.HasFlag(preference))
      {
        ProjectGenerationFlag ^= preference;
      }
      else
      {
        ProjectGenerationFlag |= preference;
      }
    }

    public void ResetProjectGenerationFlag()
    {
      ProjectGenerationFlag = ProjectGenerationFlag.None;
    }
  }
}
