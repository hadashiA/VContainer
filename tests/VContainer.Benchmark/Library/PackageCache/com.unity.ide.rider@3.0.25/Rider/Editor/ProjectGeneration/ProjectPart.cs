using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using UnityEngine;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class ProjectPart
  {
    public string Name { get; }
    public string OutputPath { get; }
    public Assembly Assembly { get; }
    public string AssetsProjectPart { get; }
    public string[] SourceFiles { get; }
    public string RootNamespace { get; }
    public Assembly[] AssemblyReferences { get; }
    public string[] CompiledAssemblyReferences { get; } 
    public string[] Defines { get; }      
    public ScriptCompilerOptions CompilerOptions { get; }

    public ProjectPart(string name, Assembly assembly, string assetsProjectPart)
    {
      Name = name;
      Assembly = assembly;
      AssetsProjectPart = assetsProjectPart;
      OutputPath = assembly != null ? assembly.outputPath : "Temp/Bin/Debug";
      SourceFiles = assembly != null ? assembly.sourceFiles : new string[0];
#if UNITY_2020_2_OR_NEWER
      RootNamespace = assembly != null ? assembly.rootNamespace : string.Empty;
#else
      RootNamespace = UnityEditor.EditorSettings.projectGenerationRootNamespace;
#endif
      AssemblyReferences = assembly != null ? assembly.assemblyReferences : new Assembly[0];
      CompiledAssemblyReferences = assembly!=null? assembly.compiledAssemblyReferences:new string[0];
      Defines = assembly != null ? assembly.defines : new string[0];
      CompilerOptions = assembly != null ? assembly.compilerOptions : new ScriptCompilerOptions();
    }

    public IEnumerable<ResponseFileData> ParseResponseFileData(IAssemblyNameProvider assemblyNameProvider, string projectDirectory)
    {
      if (Assembly == null)
        return new ResponseFileData[0];
      
      var systemReferenceDirectories =
        CompilationPipeline.GetSystemAssemblyDirectories(Assembly.compilerOptions.ApiCompatibilityLevel);

      var responseFilesData = Assembly.compilerOptions.ResponseFiles.ToDictionary(
        x => x, x => assemblyNameProvider.ParseResponseFile(
          x,
          projectDirectory,
          systemReferenceDirectories
        ));

      var responseFilesWithErrors = responseFilesData.Where(x => x.Value.Errors.Any())
        .ToDictionary(x => x.Key, x => x.Value);

      if (responseFilesWithErrors.Any())
      {
        foreach (var error in responseFilesWithErrors)
        foreach (var valueError in error.Value.Errors)
        {
          Debug.LogError($"{error.Key} Parse Error : {valueError}");
        }
      }

      return responseFilesData.Select(x => x.Value);
    }
  }
}