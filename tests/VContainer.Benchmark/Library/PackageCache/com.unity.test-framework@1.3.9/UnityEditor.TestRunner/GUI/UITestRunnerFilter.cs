using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class UITestRunnerFilter
    {
#pragma warning disable 649
        public string[] assemblyNames;
        public string[] groupNames;
        public string[] categoryNames;
        public string[] testNames;
        public bool synchronousOnly;

        public static string AssemblyNameFromPath(string path)
        {
            string output = Path.GetFileName(path);
            if (output != null && output.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return output.Substring(0, output.Length - 4);
            return output;
        }

        private bool CategoryMatches(IEnumerable<string> categories)
        {
            if (categoryNames == null || categoryNames.Length == 0)
                return true;

            foreach (string category in categories)
            {
                if (categoryNames.Contains(category))
                    return true;
            }

            return false;
        }

        private bool IDMatchesAssembly(string id)
        {
            if (AreOptionalFiltersEmpty())
                return true;

            if (assemblyNames == null || assemblyNames.Length == 0)
                return true;

            int openingBracket = id.IndexOf('[');
            int closingBracket = id.IndexOf(']');
            if (openingBracket >= 0 && openingBracket < id.Length && closingBracket > openingBracket &&
                openingBracket < id.Length)
            {
                //Some assemblies are absolute and explicitly part of the test ID e.g.
                //"[/path/to/assembly-name.dll][rest of ID ...]"
                //While some are minimal assembly names e.g.
                //"[assembly-name][rest of ID ...]"
                //Strip them down to just the assembly name
                string assemblyNameFromID =
                    AssemblyNameFromPath(id.Substring(openingBracket + 1, closingBracket - openingBracket - 1));
                foreach (string assemblyName in assemblyNames)
                {
                    if (assemblyName.Equals(assemblyNameFromID, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private bool NameMatches(string name)
        {
            if (AreOptionalFiltersEmpty())
                return true;

            if (groupNames == null || groupNames.Length == 0)
                return true;

            foreach (var nameFromFilter in groupNames)
            {
                //Strict regex match for test group name on its own
                if (Regex.IsMatch(name, nameFromFilter))
                    return true;
                //Match test names that end with parametrized test values and full nunit generated test names that have . separators
                var regex = nameFromFilter.TrimEnd('$') + @"[\.|\(.*\)]";
                if (Regex.IsMatch(name, regex))
                    return true;
            }

            return false;
        }

        private bool AreOptionalFiltersEmpty()
        {
            if (assemblyNames != null && assemblyNames.Length != 0)
                return false;
            if (groupNames != null && groupNames.Length != 0)
                return false;
            if (testNames != null && testNames.Length != 0)
                return false;
            return true;
        }

        private bool NameMatchesExactly(string name)
        {
            if (AreOptionalFiltersEmpty())
                return true;

            if (testNames == null || testNames.Length == 0)
                return true;

            foreach (var exactName in testNames)
            {
                if (name == exactName)
                    return true;
            }

            return false;
        }

        private static void ClearAncestors(IEnumerable<IClearableResult> newResultList, string parentID)
        {
            if (string.IsNullOrEmpty(parentID))
                return;
            foreach (var result in newResultList)
            {
                if (result.Id == parentID)
                {
                    result.Clear();
                    ClearAncestors(newResultList, result.ParentId);
                    break;
                }
            }
        }

        public void ClearResults(List<IClearableResult> newResultList)
        {
            foreach (var result in newResultList)
            {
                if (!result.IsSuite && CategoryMatches(result.Categories))
                {
                    if (IDMatchesAssembly(result.Id) && NameMatches(result.FullName) &&
                        NameMatchesExactly(result.FullName))
                    {
                        result.Clear();
                        ClearAncestors(newResultList, result.ParentId);
                    }
                }
            }
        }

        internal interface IClearableResult
        {
            string Id { get; }
            string FullName { get; }
            string ParentId { get; }
            bool IsSuite { get; }
            List<string> Categories { get; }
            void Clear();
        }
    }
}
