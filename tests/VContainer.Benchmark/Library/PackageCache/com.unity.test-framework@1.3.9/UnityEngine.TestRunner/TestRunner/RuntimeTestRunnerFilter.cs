using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;
using UnityEngine.TestRunner.NUnitExtensions.Filters;
using FullNameFilter = UnityEngine.TestRunner.NUnitExtensions.Filters.FullNameFilter;

namespace UnityEngine.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class RuntimeTestRunnerFilter
    {
        public string[] assemblyNames;
        public string[] groupNames;
        public string[] categoryNames;
        public string[] testNames;
        public bool synchronousOnly;
        
        public ITestFilter BuildNUnitFilter()
        {
            var filters = new List<ITestFilter>();

            AddFilters(filters, testNames, s => new FullNameFilter(s));
            AddFilters(filters, groupNames, OptimizedGroupFilter);
            AddFilters(filters, assemblyNames, s => new AssemblyNameFilter(s));
            AddFilters(filters, categoryNames, s => new CategoryFilterExtended(s) {IsRegex = true});

            if (synchronousOnly)
            {
                filters.Add(new SynchronousFilter());
            }

            return filters.Count == 0 ? TestFilter.Empty : new AndFilter(filters.ToArray());
        }

        private static FullNameFilter OptimizedGroupFilter(string s)
        {
            if (s.Length >= 2)
            {
                // A common case is that the regex we are filtering by is of the form
                //   ^JUST_A_NAME$
                // so we have a regex that matches _precisely_ one string. This can be done without a regex, which
                // is much much faster.
                if (s[0] == '^' && s[s.Length - 1] == '$')
                {
                    var raw = s.Substring(1, s.Length - 2);
                    var escaped = Regex.Escape(raw);
                    // if a regex is the same when we escape it, it means that it doesn't contain any characters
                    // with any meaning in the regex. Hence the regex is just a plain name.
                    if (raw.Equals(escaped, StringComparison.Ordinal))
                        return new FullNameFilter(raw);
                }
            }

            return new FullNameFilter(s) { IsRegex = true };
        }

        private static void AddFilters(List<ITestFilter> filters, string[] values, Func<string, TestFilter> builder)
        {
            if (values == null || values.Length == 0)
            {
                return;
            }

            var inclusionFilters = values.Where(v => !v.StartsWith("!")).Select(v => builder(v) as ITestFilter).ToArray();
            var exclusionFilters = values.Where(v => v.StartsWith("!"))
                .Select(v => new NotFilter(builder(v.Substring(1))) as ITestFilter)
                .ToArray();
            if (inclusionFilters.Length > 0 && exclusionFilters.Length > 0)
            {
                filters.Add(new AndFilter(new OrFilter(inclusionFilters), new AndFilter(exclusionFilters)));
            }
            else if (inclusionFilters.Length > 0)
            {
                filters.Add(new OrFilter(inclusionFilters));
            }
            else // Only exclusionFilters
            {
                filters.Add(new AndFilter(exclusionFilters));
            }
        }
    }
}
