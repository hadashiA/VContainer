using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Packages.Rider.Editor.Util;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class ProjectGeneration : IGenerator
  {
    private enum ScriptingLanguage
    {
      None,
      CSharp
    }

    public static readonly string MSBuildNamespaceUri = "http://schemas.microsoft.com/developer/msbuild/2003";

    /// <summary>
    /// Map source extensions to ScriptingLanguages
    /// </summary>
    private static readonly Dictionary<string, ScriptingLanguage> k_BuiltinSupportedExtensions =
      new Dictionary<string, ScriptingLanguage>
      {
        { "cs", ScriptingLanguage.CSharp },
        { "uxml", ScriptingLanguage.None },
        { "uss", ScriptingLanguage.None },
        { "shader", ScriptingLanguage.None },
        { "compute", ScriptingLanguage.None },
        { "cginc", ScriptingLanguage.None },
        { "hlsl", ScriptingLanguage.None },
        { "glslinc", ScriptingLanguage.None },
        { "template", ScriptingLanguage.None },
        { "raytrace", ScriptingLanguage.None },
        { "json", ScriptingLanguage.None},
        { "rsp", ScriptingLanguage.None},
        { "asmdef", ScriptingLanguage.None},
        { "asmref", ScriptingLanguage.None},
        { "xaml", ScriptingLanguage.None},
        { "tt", ScriptingLanguage.None},
        { "t4", ScriptingLanguage.None},
        { "ttinclude", ScriptingLanguage.None}
      };

    private string m_SolutionProjectEntryTemplate = string.Join(Environment.NewLine,
      @"Project(""{{{0}}}"") = ""{1}"", ""{2}"", ""{{{3}}}""",
      @"EndProject").Replace("    ", "\t");

    private string m_SolutionProjectConfigurationTemplate = string.Join(Environment.NewLine,
      @"        {{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU",
      @"        {{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU").Replace("    ", "\t");

    private string[] m_ProjectSupportedExtensions = new string[0];

    public string ProjectDirectory { get; }

    private readonly string m_ProjectName;
    private readonly IAssemblyNameProvider m_AssemblyNameProvider;
    private readonly IFileIO m_FileIOProvider;
    private readonly IGUIDGenerator m_GUIDGenerator;

    internal static bool isRiderProjectGeneration; // workaround to https://github.cds.internal.unity3d.com/unity/com.unity.ide.rider/issues/28

    private const string k_ToolsVersion = "4.0";
    private const string k_ProductVersion = "10.0.20506";
    private const string k_BaseDirectory = ".";
    private const string k_TargetLanguageVersion = "latest";

    IAssemblyNameProvider IGenerator.AssemblyNameProvider => m_AssemblyNameProvider;

    public ProjectGeneration()
      : this(Directory.GetParent(Application.dataPath).FullName) { }

    public ProjectGeneration(string tempDirectory)
      : this(tempDirectory, new AssemblyNameProvider(), new FileIOProvider(), new GUIDProvider()) { }

    public ProjectGeneration(string tempDirectory, IAssemblyNameProvider assemblyNameProvider, IFileIO fileIoProvider, IGUIDGenerator guidGenerator)
    {
      ProjectDirectory = tempDirectory.NormalizePath();
      m_ProjectName = Path.GetFileName(ProjectDirectory);
      m_AssemblyNameProvider = assemblyNameProvider;
      m_FileIOProvider = fileIoProvider;
      m_GUIDGenerator = guidGenerator;
    }

    /// <summary>
    /// Syncs the scripting solution if any affected files are relevant.
    /// </summary>
    /// <returns>
    /// Whether the solution was synced.
    /// </returns>
    /// <param name='affectedFiles'>
    /// A set of files whose status has changed
    /// </param>
    /// <param name="reimportedFiles">
    /// A set of files that got reimported
    /// </param>
    /// <param name="checkProjectFiles">
    /// Check if project files were changed externally
    /// </param>
    public bool SyncIfNeeded(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles, bool checkProjectFiles = false)
    {
      SetupSupportedExtensions();
      
      PackageManagerTracker.SyncIfNeeded(checkProjectFiles);

      if (HasFilesBeenModified(affectedFiles, reimportedFiles) || RiderScriptEditorData.instance.hasChanges 
                                                               || RiderScriptEditorData.instance.HasChangesInCompilationDefines()
                                                               || (checkProjectFiles && LastWriteTracker.HasLastWriteTimeChanged()))
      {
        Sync();
        return true;
      }

      return false;
    }

    private bool HasFilesBeenModified(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles)
    {
      return affectedFiles.Any(ShouldFileBePartOfSolution) || reimportedFiles.Any(ShouldSyncOnReimportedAsset);
    }

    private static bool ShouldSyncOnReimportedAsset(string asset)
    {
      var extension = Path.GetExtension(asset);
      return extension == ".asmdef" || extension == ".asmref" || Path.GetFileName(asset) == "csc.rsp";
    }

    public void Sync()
    {
      SetupSupportedExtensions();
      var types = GetAssetPostprocessorTypes();
      isRiderProjectGeneration = true;
      var externalCodeAlreadyGeneratedProjects = OnPreGeneratingCSProjectFiles(types);
      isRiderProjectGeneration = false;
      if (!externalCodeAlreadyGeneratedProjects)
      {
        GenerateAndWriteSolutionAndProjects(types);
      }

      OnGeneratedCSProjectFiles(types);
      m_AssemblyNameProvider.ResetPackageInfoCache();
      m_AssemblyNameProvider.ResetAssembliesCache();
      RiderScriptEditorData.instance.hasChanges = false;
      RiderScriptEditorData.instance.InvalidateSavedCompilationDefines();
    }

    public bool HasSolutionBeenGenerated()
    {
      return m_FileIOProvider.Exists(SolutionFile());
    }

    private void SetupSupportedExtensions()
    {
      m_ProjectSupportedExtensions = m_AssemblyNameProvider.ProjectSupportedExtensions;
    }

    private bool ShouldFileBePartOfSolution(string file)
    {
      // Exclude files coming from packages except if they are internalized.
      if (m_AssemblyNameProvider.IsInternalizedPackagePath(file))
      {
          return false;
      }
      return HasValidExtension(file);
    }

    public bool HasValidExtension(string file)
    {
      var extension = Path.GetExtension(file);

      // Dll's are not scripts but still need to be included..
      if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
          return true;

      return IsSupportedExtension(extension);
    }

    private bool IsSupportedExtension(string extension)
    {
      extension = extension.TrimStart('.');
      return k_BuiltinSupportedExtensions.ContainsKey(extension) || m_ProjectSupportedExtensions.Contains(extension);
    }

    public void GenerateAndWriteSolutionAndProjects(Type[] types)
    {
      // Only synchronize islands that have associated source files and ones that we actually want in the project.
      // This also filters out DLLs coming from .asmdef files in packages.
      var assemblies = m_AssemblyNameProvider.GetAssemblies(ShouldFileBePartOfSolution).ToArray();
      var allAssetProjectParts = GenerateAllAssetProjectParts();

      var projectParts = new List<ProjectPart>();
      var visitedAssemblyNames = new HashSet<string>();
      foreach (var assembly in assemblies)
      {
        if (visitedAssemblyNames.Contains(assembly.name))
          projectParts.Add(new ProjectPart(assembly.name, assembly, string.Empty)); // do not add asset project parts to both editor and player projects
        else
        {
          allAssetProjectParts.TryGetValue(assembly.name, out var additionalAssetsForProject);
          projectParts.Add(new ProjectPart(assembly.name, assembly, additionalAssetsForProject));
          visitedAssemblyNames.Add(assembly.name);  
        }
      }

      var executingAssemblyName = typeof(ProjectGeneration).Assembly.GetName().Name;
      var riderAssembly = m_AssemblyNameProvider.GetAssemblies(_ => true).FirstOrDefault(a=>a.name == executingAssemblyName);
      var projectPartsWithoutAssembly = allAssetProjectParts.Where(a => !visitedAssemblyNames.Contains(a.Key));
      projectParts.AddRange(projectPartsWithoutAssembly.Select(allAssetProjectPart => 
        AddProjectPart(allAssetProjectPart.Key, riderAssembly, allAssetProjectPart.Value)));

      SyncSolution(projectParts.ToArray(), types);

      foreach (var projectPart in projectParts)
      {
        SyncProject(projectPart, types);
      }
    }

    private static ProjectPart AddProjectPart(string assemblyName, Assembly riderAssembly, string assetProjectPart)
    {
      Assembly assembly = null;
      if (riderAssembly != null)
        // We want to add those references, so that Rider would detect Unity path and version and provide rich features for shader files
        assembly = new Assembly(assemblyName, riderAssembly.outputPath, Array.Empty<string>(),
          new []{"UNITY_EDITOR"},
          Array.Empty<Assembly>(),
          riderAssembly.compiledAssemblyReferences.Where(a =>
            a.EndsWith("UnityEditor.dll", StringComparison.Ordinal) || a.EndsWith("UnityEngine.dll", StringComparison.Ordinal) ||
            a.EndsWith("UnityEngine.CoreModule.dll", StringComparison.Ordinal)).ToArray(), riderAssembly.flags);
      return new ProjectPart(assemblyName, assembly, assetProjectPart);
    }

    private Dictionary<string, string> GenerateAllAssetProjectParts()
    {
      var stringBuilders = new Dictionary<string, StringBuilder>();

      foreach (var asset in m_AssemblyNameProvider.GetAllAssetPaths())
      {
        // Exclude files coming from packages except if they are internalized.
        if (m_AssemblyNameProvider.IsInternalizedPackagePath(asset))
        {
          continue;
        }

        var fallbackAssemblyName = "Assembly-CSharp";
        var extension = Path.GetExtension(asset);
        if (AssetDatabase.IsValidFolder(asset))
        {
          var assemblyName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath($"{asset}/asset.cs");

          if (string.IsNullOrEmpty(assemblyName))
          {
            assemblyName = fallbackAssemblyName;
          }

          assemblyName = FileSystemUtil.FileNameWithoutExtension(assemblyName);

          if (!stringBuilders.TryGetValue(assemblyName, out var projectBuilder))
          {
            projectBuilder = new StringBuilder();
            stringBuilders[assemblyName] = projectBuilder;
          }

          projectBuilder.Append("     <Folder Include=\"").Append(m_FileIOProvider.EscapedRelativePathFor(asset, ProjectDirectory)).Append("\" />")
            .Append(Environment.NewLine);
        }
        else if (IsSupportedExtension(extension) && !extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
          // Find assembly the asset belongs to by adding script extension and using compilation pipeline.
          var assemblyName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath(asset + ".cs");

          if (string.IsNullOrEmpty(assemblyName))
          {
            assemblyName = fallbackAssemblyName;
          }

          assemblyName = FileSystemUtil.FileNameWithoutExtension(assemblyName);

          if (!stringBuilders.TryGetValue(assemblyName, out var projectBuilder))
          {
            projectBuilder = new StringBuilder();
            stringBuilders[assemblyName] = projectBuilder;
          }

          projectBuilder.Append("     <None Include=\"").Append(m_FileIOProvider.EscapedRelativePathFor(asset, ProjectDirectory)).Append("\" />")
            .Append(Environment.NewLine);
        }
      }

      var result = new Dictionary<string, string>();

      foreach (var entry in stringBuilders)
        result[entry.Key] = entry.Value.ToString();

      return result;
    }

    private void SyncProject(
      ProjectPart island,
      Type[] types)
    {
      SyncProjectFileIfNotChanged(
        ProjectFile(island),
        ProjectText(island),
        types);
    }

    private void SyncProjectFileIfNotChanged(string path, string newContents, Type[] types)
    {
      if (Path.GetExtension(path) == ".csproj")
      {
        newContents = OnGeneratedCSProject(path, newContents, types);
      }

      SyncFileIfNotChanged(path, newContents);
    }

    private void SyncSolutionFileIfNotChanged(string path, string newContents, Type[] types)
    {
      newContents = OnGeneratedSlnSolution(path, newContents, types);

      SyncFileIfNotChanged(path, newContents);
    }

    private static void OnGeneratedCSProjectFiles(Type[] types)
    {
      foreach (var type in types)
      {
        var method = type.GetMethod("OnGeneratedCSProjectFiles",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        Debug.LogWarning("OnGeneratedCSProjectFiles is not supported.");
        // RIDER-51958
        //method.Invoke(null, args);
      }
    }

    public static Type[] GetAssetPostprocessorTypes()
    {
      return TypeCache.GetTypesDerivedFrom<AssetPostprocessor>().ToArray(); // doesn't find types from EditorPlugin, which is fine
    }

    private static bool OnPreGeneratingCSProjectFiles(Type[] types)
    {
      var result = false;
      foreach (var type in types)
      {
        var args = new object[0];
        var method = type.GetMethod("OnPreGeneratingCSProjectFiles",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(bool))
        {
          result |= (bool)returnValue;
        }
      }

      return result;
    }

    private static string OnGeneratedCSProject(string path, string content, Type[] types)
    {
      foreach (var type in types)
      {
        var args = new[] { path, content };
        var method = type.GetMethod("OnGeneratedCSProject",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(string))
        {
          content = (string)returnValue;
        }
      }

      return content;
    }

    private static string OnGeneratedSlnSolution(string path, string content, Type[] types)
    {
      foreach (var type in types)
      {
        var args = new[] { path, content };
        var method = type.GetMethod("OnGeneratedSlnSolution",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(string))
        {
          content = (string)returnValue;
        }
      }

      return content;
    }

    private void SyncFileIfNotChanged(string path, string newContents)
    {
      try
      {
        if (m_FileIOProvider.Exists(path) && newContents == m_FileIOProvider.ReadAllText(path))
        {
          return;
        }
      }
      catch (Exception exception)
      {
        Debug.LogException(exception);
      }

      m_FileIOProvider.WriteAllText(path, newContents);
    }

    private string ProjectText(ProjectPart assembly)
    {
      var responseFilesData = assembly.ParseResponseFileData(m_AssemblyNameProvider, ProjectDirectory).ToList();
      var projectBuilder = new StringBuilder(ProjectHeader(assembly, responseFilesData));

      foreach (var file in assembly.SourceFiles)
      {
        var fullFile = m_FileIOProvider.EscapedRelativePathFor(file, ProjectDirectory);
        projectBuilder.Append("     <Compile Include=\"").Append(fullFile).Append("\" />").Append(Environment.NewLine);
      }

      projectBuilder.Append(assembly.AssetsProjectPart);

      var responseRefs = responseFilesData.SelectMany(x => x.FullPathReferences.Select(r => r));
      var internalAssemblyReferences = assembly.AssemblyReferences
        .Where(reference => !reference.sourceFiles.Any(ShouldFileBePartOfSolution)).Select(i => i.outputPath);
      var allReferences =
        assembly.CompiledAssemblyReferences
          .Union(responseRefs)
          .Union(internalAssemblyReferences).ToArray();

      foreach (var reference in allReferences)
      {
        var fullReference = Path.IsPathRooted(reference) ? reference : Path.GetFullPath(reference);
        AppendReference(fullReference, projectBuilder);
      }

      if (0 < assembly.AssemblyReferences.Length)
      {
        projectBuilder.Append("  </ItemGroup>").Append(Environment.NewLine);
        projectBuilder.Append("  <ItemGroup>").Append(Environment.NewLine);
        foreach (var reference in assembly.AssemblyReferences.Where(i => i.sourceFiles.Any(ShouldFileBePartOfSolution)))
        {
          var name = m_AssemblyNameProvider.GetProjectName(reference.name, reference.defines);
          projectBuilder.Append("    <ProjectReference Include=\"").Append(name).Append(GetProjectExtension()).Append("\">").Append(Environment.NewLine);
          projectBuilder.Append("      <Project>{").Append(ProjectGuid(name)).Append("}</Project>").Append(Environment.NewLine);
          projectBuilder.Append("      <Name>").Append(name).Append("</Name>").Append(Environment.NewLine);
          projectBuilder.Append("    </ProjectReference>").Append(Environment.NewLine);
        }
      }

      projectBuilder.Append(ProjectFooter());
      return projectBuilder.ToString();
    }

    private static void AppendReference(string fullReference, StringBuilder projectBuilder)
    {
      var escapedFullPath = SecurityElement.Escape(fullReference);
      escapedFullPath = escapedFullPath.NormalizePath();
      projectBuilder.Append("     <Reference Include=\"").Append(FileSystemUtil.FileNameWithoutExtension(escapedFullPath))
        .Append("\">").Append(Environment.NewLine);
      projectBuilder.Append("     <HintPath>").Append(escapedFullPath).Append("</HintPath>").Append(Environment.NewLine);
      projectBuilder.Append("     </Reference>").Append(Environment.NewLine);
    }

    private string ProjectFile(ProjectPart projectPart)
    {
      return Path.Combine(ProjectDirectory, $"{m_AssemblyNameProvider.GetProjectName(projectPart.Name, projectPart.Defines)}.csproj");
    }

    public string SolutionFile()
    {
      return Path.Combine(ProjectDirectory, $"{m_ProjectName}.sln");
    }

    private string ProjectHeader(
      ProjectPart assembly,
      List<ResponseFileData> responseFilesData
    )
    {
      var otherResponseFilesData = GetOtherArgumentsFromResponseFilesData(responseFilesData);
      var arguments = new object[]
      {
        k_ToolsVersion,
        k_ProductVersion,
        ProjectGuid(m_AssemblyNameProvider.GetProjectName(assembly.Name, assembly.Defines)),
        InternalEditorUtility.GetEngineAssemblyPath(),
        InternalEditorUtility.GetEditorAssemblyPath(),
        string.Join(";", assembly.Defines.Concat(responseFilesData.SelectMany(x => x.Defines)).Distinct().ToArray()),
        MSBuildNamespaceUri,
        assembly.Name,
        assembly.OutputPath,
        assembly.RootNamespace,
        assembly.CompilerOptions.ApiCompatibilityLevel,
        GenerateLangVersion(otherResponseFilesData["langversion"], assembly),
        k_BaseDirectory,
        assembly.CompilerOptions.AllowUnsafeCode | responseFilesData.Any(x => x.Unsafe),
        GenerateNoWarn(otherResponseFilesData["nowarn"].Distinct().ToList()),
        GenerateAnalyserItemGroup(RetrieveRoslynAnalyzers(assembly, otherResponseFilesData)),
        GenerateAnalyserAdditionalFiles(RetrieveAdditionalFiles(assembly, otherResponseFilesData)),
        GenerateRoslynAnalyzerRulesetPath(assembly, otherResponseFilesData),
        GenerateWarningLevel(otherResponseFilesData["warn"].Concat(otherResponseFilesData["w"]).Distinct()),
        GenerateWarningAsError(otherResponseFilesData["warnaserror"], otherResponseFilesData["warnaserror-"], otherResponseFilesData["warnaserror+"]),
        GenerateDocumentationFile(otherResponseFilesData["doc"].ToArray()),
        GenerateNullable(otherResponseFilesData["nullable"]),
        GenerateGlobalAnalyzerConfigFiles(assembly)
      };

      try
      {
        return string.Format(GetProjectHeaderTemplate(), arguments);
      }
      catch (Exception)
      {
        throw new NotSupportedException(
          "Failed creating c# project because the c# project header did not have the correct amount of arguments, which is " +
          arguments.Length);
      }
    }

    private static string GenerateGlobalAnalyzerConfigFiles(ProjectPart assembly)
    {
      var configFile = string.Empty;
#if UNITY_2021_3 // https://github.com/JetBrains/resharper-unity/issues/2401
      var type = assembly.CompilerOptions.GetType();
      var propertyInfo = type.GetProperty("AnalyzerConfigPath");
      if (propertyInfo != null && propertyInfo.GetValue(assembly.CompilerOptions) is string value)
      {
        configFile = value;
      }
#elif UNITY_2022_2_OR_NEWER
        configFile = assembly.CompilerOptions.AnalyzerConfigPath; // https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Compilation.ScriptCompilerOptions.AnalyzerConfigPath.html
#endif
      
      if (string.IsNullOrEmpty(configFile))
        return string.Empty;
      
      var builder = new StringBuilder();
      builder.AppendLine("  <ItemGroup>");
      builder.AppendLine($"    <GlobalAnalyzerConfigFiles Include=\"{configFile}\" />");
      builder.AppendLine("  </ItemGroup>");
      return builder.ToString();
    }

    private static string[] RetrieveAdditionalFiles(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
      var additionalFilePathsFromCompilationPipeline = Array.Empty<string>();
#if UNITY_2021_3 // https://github.com/JetBrains/resharper-unity/issues/2401
      var type = assembly.CompilerOptions.GetType();
      var propertyInfo = type.GetProperty("RoslynAdditionalFilePaths");
      if (propertyInfo != null && propertyInfo.GetValue(assembly.CompilerOptions) is string[] value)
      {
        additionalFilePathsFromCompilationPipeline = value;
      }
#elif UNITY_2022_2_OR_NEWER // https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Compilation.ScriptCompilerOptions.RoslynAdditionalFilePaths.html
        additionalFilePathsFromCompilationPipeline = assembly.CompilerOptions.RoslynAdditionalFilePaths;
#endif
      return otherResponseFilesData["additionalfile"]
        .SelectMany(x=>x.Split(';'))
        .Concat(additionalFilePathsFromCompilationPipeline)
        .Distinct().ToArray();
    }

    string[] RetrieveRoslynAnalyzers(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
#if UNITY_2020_2_OR_NEWER
      return otherResponseFilesData["analyzer"].Concat(otherResponseFilesData["a"])
        .SelectMany(x=>x.Split(';'))
#if !ROSLYN_ANALYZER_FIX
        .Concat(m_AssemblyNameProvider.GetRoslynAnalyzerPaths())
#else
        .Concat(assembly.CompilerOptions.RoslynAnalyzerDllPaths)
#endif
        .Select(MakeAbsolutePath)
        .Distinct()
        .ToArray();
#else
      return otherResponseFilesData["analyzer"].Concat(otherResponseFilesData["a"])
        .SelectMany(x=>x.Split(';'))
        .Distinct()
        .Select(MakeAbsolutePath)
        .ToArray();
#endif
    }

    private static string MakeAbsolutePath(string path)
    {
      return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
    }

    private string GenerateRoslynAnalyzerRulesetPath(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
#if UNITY_2020_2_OR_NEWER
      return GenerateAnalyserRuleSet(otherResponseFilesData["ruleset"].Append(assembly.CompilerOptions.RoslynAnalyzerRulesetPath).Where(a=>!string.IsNullOrEmpty(a)).Distinct().Select(x => MakeAbsolutePath(x).NormalizePath()).ToArray());
#else
      return GenerateAnalyserRuleSet(otherResponseFilesData["ruleset"].Distinct().Select(x => MakeAbsolutePath(x).NormalizePath()).ToArray());
#endif
    }

    private string GenerateNullable(IEnumerable<string> enumerable)
    {
      var val = enumerable.FirstOrDefault();
      if (string.IsNullOrWhiteSpace(val)) 
        return string.Empty;
      
      return $"{Environment.NewLine}    <Nullable>{val}</Nullable>";
    }

    private static string GenerateDocumentationFile(string[] paths)
    {
      if (!paths.Any())
        return String.Empty;

      return $"{Environment.NewLine}{string.Join(Environment.NewLine, paths.Select(a => $"  <DocumentationFile>{a}</DocumentationFile>"))}";
    }

    private static string GenerateWarningAsError(IEnumerable<string> args, IEnumerable<string> argsMinus, IEnumerable<string> argsPlus)
    {
      var returnValue = String.Empty;
      var allWarningsAsErrors = false;
      var warningIds = new List<string>();

      foreach (var s in args)
      {
        if (s == "+" || s == string.Empty) allWarningsAsErrors = true;
        else if (s == "-") allWarningsAsErrors = false;
        else
        {
          warningIds.Add(s);
        }
      }
      
      warningIds.AddRange(argsPlus);

      returnValue += $@"    <TreatWarningsAsErrors>{allWarningsAsErrors}</TreatWarningsAsErrors>";
      if (warningIds.Any())
      {
        returnValue += $"{Environment.NewLine}    <WarningsAsErrors>{string.Join(";", warningIds)}</WarningsAsErrors>";
      }

      if (argsMinus.Any())
        returnValue += $"{Environment.NewLine}    <WarningsNotAsErrors>{string.Join(";", argsMinus)}</WarningsNotAsErrors>";
      
      return $"{Environment.NewLine}{returnValue}";
    }

    private static string GenerateWarningLevel(IEnumerable<string> warningLevel)
    {
      var level = warningLevel.FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(level))
        return level;

      return 4.ToString();
    }

    private static string GetSolutionText()
    {
      return string.Join(Environment.NewLine,
        @"",
        @"Microsoft Visual Studio Solution File, Format Version {0}",
        @"# Visual Studio {1}",
        @"{2}",
        @"Global",
        @"    GlobalSection(SolutionConfigurationPlatforms) = preSolution",
        @"        Debug|Any CPU = Debug|Any CPU",
        @"    EndGlobalSection",
        @"    GlobalSection(ProjectConfigurationPlatforms) = postSolution",
        @"{3}",
        @"    EndGlobalSection",
        @"    GlobalSection(SolutionProperties) = preSolution",
        @"        HideSolutionNode = FALSE",
        @"    EndGlobalSection",
        @"EndGlobal",
        @"").Replace("    ", "\t");
    }

    private static string GetProjectFooterTemplate()
    {
      return string.Join(Environment.NewLine,
        @"  </ItemGroup>",
        @"  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />",
        @"  <!-- To modify your build process, add your task inside one of the targets below and uncomment it.",
        @"       Other similar extension points exist, see Microsoft.Common.targets.",
        @"  <Target Name=""BeforeBuild"">",
        @"  </Target>",
        @"  <Target Name=""AfterBuild"">",
        @"  </Target>",
        @"  -->",
        @"</Project>",
        @"");
    }

    private static string GetProjectHeaderTemplate()
    {
      var header = new[]
      {
        @"<?xml version=""1.0"" encoding=""utf-8""?>",
        @"<Project ToolsVersion=""{0}"" DefaultTargets=""Build"" xmlns=""{6}"">",
        @"  <PropertyGroup>",
        @"    <LangVersion>{11}</LangVersion>",
        @"    <_TargetFrameworkDirectories>non_empty_path_generated_by_unity.rider.package</_TargetFrameworkDirectories>",
        @"    <_FullFrameworkReferenceAssemblyPaths>non_empty_path_generated_by_unity.rider.package</_FullFrameworkReferenceAssemblyPaths>",
        @"    <DisableHandlePackageFileConflicts>true</DisableHandlePackageFileConflicts>{17}",
        @"  </PropertyGroup>",
        @"  <PropertyGroup>",
        @"    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>",
        @"    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>",
        @"    <ProductVersion>{1}</ProductVersion>",
        @"    <SchemaVersion>2.0</SchemaVersion>",
        @"    <RootNamespace>{9}</RootNamespace>",
        @"    <ProjectGuid>{{{2}}}</ProjectGuid>",
        @"    <ProjectTypeGuids>{{E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1}};{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}</ProjectTypeGuids>",
        @"    <OutputType>Library</OutputType>",
        @"    <AppDesignerFolder>Properties</AppDesignerFolder>",
        @"    <AssemblyName>{7}</AssemblyName>",
        @"    <TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>",
        @"    <FileAlignment>512</FileAlignment>",
        @"    <BaseDirectory>{12}</BaseDirectory>",
        @"  </PropertyGroup>",
        @"  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">",
        @"    <DebugSymbols>true</DebugSymbols>",
        @"    <DebugType>full</DebugType>",
        @"    <Optimize>false</Optimize>",
        @"    <OutputPath>{8}</OutputPath>",
        @"    <DefineConstants>{5}</DefineConstants>",
        @"    <ErrorReport>prompt</ErrorReport>",
        @"    <WarningLevel>{18}</WarningLevel>",
        @"    <NoWarn>{14}</NoWarn>",
        @"    <AllowUnsafeBlocks>{13}</AllowUnsafeBlocks>{19}{20}{21}",
        @"  </PropertyGroup>"
      };

      var forceExplicitReferences = new[]
      {
        @"  <PropertyGroup>",
        @"    <NoConfig>true</NoConfig>",
        @"    <NoStdLib>true</NoStdLib>",
        @"    <AddAdditionalExplicitAssemblyReferences>false</AddAdditionalExplicitAssemblyReferences>",
        @"    <ImplicitlyExpandNETStandardFacades>false</ImplicitlyExpandNETStandardFacades>",
        @"    <ImplicitlyExpandDesignTimeFacades>false</ImplicitlyExpandDesignTimeFacades>",
        @"  </PropertyGroup>"
      };

      var footer = new[]
      {
        @"{15}{16}{22}  <ItemGroup>",
        @""
      };

      var pieces = header.Concat(forceExplicitReferences).Concat(footer).ToArray();
      return string.Join(Environment.NewLine, pieces);
    }

    private void SyncSolution(ProjectPart[] islands, Type[] types)
    {
      SyncSolutionFileIfNotChanged(SolutionFile(), SolutionText(islands), types);
    }

    private string SolutionText(ProjectPart[] islands)
    {
      var fileversion = "11.00";
      var vsversion = "2010";
      
      var projectEntries = GetProjectEntries(islands);
      var projectConfigurations = string.Join(Environment.NewLine,
        islands.Select(i => GetProjectActiveConfigurations(ProjectGuid(m_AssemblyNameProvider.GetProjectName(i.Name, i.Defines)))).ToArray());
      return string.Format(GetSolutionText(), fileversion, vsversion, projectEntries, projectConfigurations);
    }

    private static string GenerateAnalyserItemGroup(string[] paths)
    {
      //   <ItemGroup>
      //      <Analyzer Include="..\packages\Comments_analyser.1.0.6626.21356\analyzers\dotnet\cs\Comments_analyser.dll" />
      //      <Analyzer Include="..\packages\UnityEngineAnalyzer.1.0.0.0\analyzers\dotnet\cs\UnityEngineAnalyzer.dll" />
      //  </ItemGroup>
      if (!paths.Any())
        return string.Empty;

      var analyserBuilder = new StringBuilder();
      analyserBuilder.AppendLine("  <ItemGroup>");
      foreach (var path in paths)
      {
        analyserBuilder.AppendLine($"    <Analyzer Include=\"{path.NormalizePath()}\" />");
      }

      analyserBuilder.AppendLine("  </ItemGroup>");
      return analyserBuilder.ToString();
    }

    private static ILookup<string, string> GetOtherArgumentsFromResponseFilesData(List<ResponseFileData> responseFilesData)
    {
      var paths = responseFilesData.SelectMany(x =>
        {
          return x.OtherArguments
            .Where(a => a.StartsWith("/", StringComparison.Ordinal) || a.StartsWith("-", StringComparison.Ordinal))
            .Select(b =>
            {
              var index = b.IndexOf(":", StringComparison.Ordinal);
              if (index > 0 && b.Length > index)
              {
                var key = b.Substring(1, index - 1);
                return new KeyValuePair<string, string>(key, b.Substring(index + 1));
              }

              const string warnaserror = "warnaserror";
              if (b.Substring(1).StartsWith(warnaserror, StringComparison.Ordinal))
              {
                return new KeyValuePair<string, string>(warnaserror, b.Substring(warnaserror.Length + 1));
              }
              const string nullable = "nullable";
              if (b.Substring(1).StartsWith(nullable, StringComparison.Ordinal))
              {
                var res = b.Substring(nullable.Length + 1);
                if (string.IsNullOrWhiteSpace(res) || res.Equals("+"))
                  res = "enable";
                else if (res.Equals("-"))
                  res = "disable";
                return new KeyValuePair<string, string>(nullable, res);
              }

              return default;
            });
        })
        .Distinct()
        .ToLookup(o => o.Key, pair => pair.Value);
      return paths;
    }

    private string GenerateLangVersion(IEnumerable<string> langVersionList, ProjectPart assembly)
    {
      var langVersion = langVersionList.FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(langVersion))
        return langVersion;
#if UNITY_2020_2_OR_NEWER
      return assembly.CompilerOptions.LanguageVersion;
#else
      return k_TargetLanguageVersion;
#endif
    }

    private static string GenerateAnalyserRuleSet(string[] paths)
    {
      //<CodeAnalysisRuleSet>..\path\to\myrules.ruleset</CodeAnalysisRuleSet>
      if (!paths.Any())
        return string.Empty;

      return $"{Environment.NewLine}{string.Join(Environment.NewLine, paths.Select(a => $"    <CodeAnalysisRuleSet>{a}</CodeAnalysisRuleSet>"))}";
    }

    private static string GenerateAnalyserAdditionalFiles(string[] paths)
    {
      if (!paths.Any())
        return string.Empty;

      var analyserBuilder = new StringBuilder();
      analyserBuilder.AppendLine("  <ItemGroup>");
      foreach (var path in paths)
      {
        analyserBuilder.AppendLine($"    <AdditionalFiles Include=\"{path}\" />");
      }

      analyserBuilder.AppendLine("  </ItemGroup>");
      return analyserBuilder.ToString();
    }

    public static string GenerateNoWarn(List<string> codes)
    {
#if UNITY_2020_1 // RIDER-77206 Unity 2020.1.3 'PlayerSettings' does not contain a definition for 'suppressCommonWarnings'
      var type = typeof(PlayerSettings);
      var propertyInfo = type.GetProperty("suppressCommonWarnings");
      if (propertyInfo != null && propertyInfo.GetValue(null) is bool && (bool)propertyInfo.GetValue(null))
      {
        codes.AddRange(new[] {"0169", "0649"});  
      }
#elif UNITY_2020_2_OR_NEWER
      if (PlayerSettings.suppressCommonWarnings)
        codes.AddRange(new[] {"0169", "0649"});
#endif

      if (!codes.Any())
        return string.Empty;

      return string.Join(",", codes.Distinct());
    }
    
    private string GetProjectEntries(ProjectPart[] islands)
    {
      var projectEntries = islands.Select(i => string.Format(
        m_SolutionProjectEntryTemplate,
        SolutionGuidGenerator.GuidForSolution(),
        i.Name,
        Path.GetFileName(ProjectFile(i)),
        ProjectGuid(m_AssemblyNameProvider.GetProjectName(i.Name, i.Defines))
      ));

      return string.Join(Environment.NewLine, projectEntries.ToArray());
    }

    /// <summary>
    /// Generate the active configuration string for a given project guid
    /// </summary>
    private string GetProjectActiveConfigurations(string projectGuid)
    {
      return string.Format(
        m_SolutionProjectConfigurationTemplate,
        projectGuid);
    }

    private static string ProjectFooter()
    {
      return GetProjectFooterTemplate();
    }

    private static string GetProjectExtension()
    {
      return ".csproj";
    }

    private string ProjectGuid(string name)
    {
      return m_GUIDGenerator.ProjectGuid(m_ProjectName + name);
    }
  }
}
