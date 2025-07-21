using System;
using System.IO;
using System.Security;
using System.Text;
using Packages.Rider.Editor.Util;

namespace Packages.Rider.Editor.ProjectGeneration {
  class FileIOProvider : IFileIO
  {
    public bool Exists(string fileName)
    {
      return File.Exists(fileName);
    }

    public string ReadAllText(string fileName)
    {
      return File.ReadAllText(fileName);
    }

    public void WriteAllText(string path, string content)
    {
      File.WriteAllText(path, content, Encoding.UTF8);
      LastWriteTracker.UpdateLastWriteIfNeeded(path);
    }

    public string EscapedRelativePathFor(string file, string projectDirectory)
    {
      var projectDir = Path.GetFullPath(projectDirectory);

      // We have to normalize the path, because the PackageManagerRemapper assumes
      // dir seperators will be os specific.
      var absolutePath = Path.GetFullPath(file.NormalizePath());
      var path = SkipPathPrefix(absolutePath, projectDir);

      return SecurityElement.Escape(path);
    }

    private static string SkipPathPrefix(string path, string prefix)
    {
      return path.StartsWith($@"{prefix}{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        ? path.Substring(prefix.Length + 1)
        : path;
    }
  }
}
