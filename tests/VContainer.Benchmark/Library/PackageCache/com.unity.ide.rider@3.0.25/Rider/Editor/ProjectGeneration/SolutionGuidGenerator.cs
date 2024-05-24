using System;
using System.Security.Cryptography;
using System.Text;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal static class SolutionGuidGenerator
  {
    public static string GuidForProject(string projectName)
    {
      return ComputeGuidHashFor(projectName + "salt");
    }

    public static string GuidForSolution()
    {
      // GUID for a C# class library: http://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs
      return "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
    }

    static string ComputeGuidHashFor(string input)
    {
      using (var md5 = MD5.Create())
      {
        var hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
        return new Guid(hash).ToString();
      }
    }
  }
}