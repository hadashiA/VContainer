using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.GUI;
using UnityEngine;

namespace UnityEditor.TestRunner.TestLaunchers
{
    internal static class FilePathMetaInfo
    {
        [Serializable]
        private struct FileReference
        {
            public string FilePath;
            public int LineNumber;
        }

        private enum PathType
        {
            ProjectRepositoryPath,
            ProjectPath,
        }

        public static void TryCreateFile(ITest runnerLoadedTest, BuildPlayerOptions playerBuildOptions)
        {
            try
            {
                var metaFileDestinationPath = GetMetaDestinationPath(playerBuildOptions);
                var repositoryPath = GetPathFromArgs(PathType.ProjectRepositoryPath);
                // if no path is given, early out so we do not pollute the build player folder with the file path file.
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return;
                }

                // Create a dictionary for the test names and their file paths
                var testFilePaths = new Dictionary<string, FileReference>();
                RecursivelyPopulateFileReferences(runnerLoadedTest, testFilePaths, repositoryPath, new GuiHelper(new MonoCecilHelper(), new AssetsDatabaseHelper()));
                SaveToJsonFile(testFilePaths, metaFileDestinationPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Saving test file path meta info failed: " + e.Message);
            }
        }

        // This function serializes dictionary to json file, all the logic would not be necessary if Unity was able to serialize Dictionaries, or if we could use Newtonsoft.Json.
        // This function could be changed later on, or we can use different data structure than Dictionary.
        private static void SaveToJsonFile(Dictionary<string, FileReference> testFilePaths, string metaFileDestinationPath)
        {
            using (var fileStream = File.CreateText(Path.Combine(metaFileDestinationPath, "TestFileReferences.json")))
            {
                fileStream.WriteLine("{");

                foreach (var testFilePath in testFilePaths)
                {
                    fileStream.WriteLine($"   \"{JavaScriptStringEncode(testFilePath.Key)}\": {{");
                    fileStream.WriteLine($"      \"filePath\": \"{JavaScriptStringEncode(testFilePath.Value.FilePath)}\",");
                    fileStream.WriteLine($"      \"lineNumber\": {testFilePath.Value.LineNumber}");
                    // check if it is the last element in the dictionary
                    if (testFilePath.Key != testFilePaths.Keys.Last())
                    {
                        fileStream.WriteLine("   },");
                    }
                    else
                    {
                        fileStream.WriteLine("   }");
                    }
                }

                fileStream.WriteLine("}");
            }
        }

        private static string GetMetaDestinationPath(BuildPlayerOptions playerBuildOptions)
        {
            // If we are Auto-Running the player, use project path instead of player build path because it will be wiped out after successful run.
            if ((playerBuildOptions.options & BuildOptions.AutoRunPlayer) != 0)
            {
                return Path.Combine(GetPathFromArgs(PathType.ProjectPath));
            }

            // if the buildOutputPath is for a file, then get the directory of it
            return File.Exists(playerBuildOptions.locationPathName) ? Path.GetDirectoryName(playerBuildOptions.locationPathName) : playerBuildOptions.locationPathName;
        }

        private static void RecursivelyPopulateFileReferences(ITest test, Dictionary<string, FileReference> testFilePaths, string repositoryPath, IGuiHelper guiHelper)
        {
            if (test.HasChildren)
            {
                foreach (var child in test.Tests)
                {
                    RecursivelyPopulateFileReferences(child, testFilePaths, repositoryPath, guiHelper);
                }

                return;
            }

            var testMethod = test.Method;
            if (testMethod == null)
            {
                testMethod = test.Parent.Method;
                if (testMethod == null)
                {
                    return;
                }
            }

            var methodInfo = test.Method.MethodInfo;
            var type = test.TypeInfo.Type;
            var fileOpenInfo = guiHelper.GetFileOpenInfo(type, methodInfo);
            var filePathString = Path.Combine(repositoryPath, fileOpenInfo.FilePath);
            var lineNumber = fileOpenInfo.LineNumber;
            var fileReference = new FileReference
            {
                FilePath = filePathString,
                LineNumber = lineNumber
            };
            // Cannot be simplified with .TryAdd because Unity 2020.3 and below does not have it.
            if (!testFilePaths.ContainsKey(test.FullName))
            {
                testFilePaths.Add(test.FullName, fileReference);
            }
        }

        private static string GetPathFromArgs(PathType type)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();

            string lookFor;
            switch (type)
            {
                case PathType.ProjectRepositoryPath:
                    lookFor = "-projectRepositoryPath";
                    break;
                case PathType.ProjectPath:
                    lookFor = "-projectPath";
                    break;
                default:
                    throw new ArgumentException("Invalid PathType");
            }

            for (var i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i].Equals(lookFor))
                {
                    return commandLineArgs[i + 1];
                }
            }

            return string.Empty;
        }
        
        // Below implementation is copy-paste from HttpUtility.JavaScriptStringEncode
        private static string JavaScriptStringEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }

            StringBuilder b = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++) {
                char c = value[i];

                // Append the unhandled characters (that do not require special treament)
                // to the string builder when special characters are detected.
                if (CharRequiresJavaScriptEncoding(c)) {
                    if (b == null) {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    startIndex = i + 1;
                    count = 0;
                }

                switch (c) {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\b':
                        b.Append("\\b");
                        break;
                    case '\f':
                        b.Append("\\f");
                        break;
                    default:
                        if (CharRequiresJavaScriptEncoding(c)) {
                            AppendCharAsUnicodeJavaScript(b, c);
                        }
                        else {
                            count++;
                        }
                        break;
                }
            }

            if (b == null) {
                return value;
            }

            if (count > 0) {
                b.Append(value, startIndex, count);
            }

            return b.ToString();
        }
        
        private static bool CharRequiresJavaScriptEncoding(char c) {
            return c < 0x20 // control chars always have to be encoded
                   || c == '\"' // chars which must be encoded per JSON spec
                   || c == '\\'
                   || c == '\'' // HTML-sensitive chars encoded for safety
                   || c == '<'
                   || c == '>'
                   || c == '&'
                   || c == '\u0085' // newline chars (see Unicode 6.2, Table 5-1 [http://www.unicode.org/versions/Unicode6.2.0/ch05.pdf]) have to be encoded (DevDiv #663531)
                   || c == '\u2028'
                   || c == '\u2029';
        }
        
        private static void AppendCharAsUnicodeJavaScript(StringBuilder builder, char c) {
            builder.Append("\\u");
            builder.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
        }
    }
}