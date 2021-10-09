using System.Collections.Generic;
using System.Linq;

namespace VContainer.Diagnostics
{
    public static class DiagnositcsContext
    {
        static readonly Dictionary<string, DiagnosticsCollector> collectors
            = new Dictionary<string, DiagnosticsCollector>();

        public static DiagnosticsCollector GetCollector(string name)
        {
            lock (collectors)
            {
                if (!collectors.TryGetValue(name, out var collector))
                {
                    collector = new DiagnosticsCollector(name);
                    collectors.Add(name, collector);
                }
                return collector;
            }
        }

        public static ILookup<string, DiagnosticsInfo> GetGroupedDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors
                    .SelectMany(x => x.Value.GetDiagnosticsInfos())
                    .Where(x => x.ResolveInfo.MaxDepth <= 1)
                    .ToLookup(x => x.ScopeName);
            }
        }

        public static IEnumerable<DiagnosticsInfo> GetDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors.SelectMany(x => x.Value.GetDiagnosticsInfos());
            }
        }

        internal static DiagnosticsInfo FindByRegistration(IRegistration registration)
        {
            return GetDiagnosticsInfos().FirstOrDefault(x => x.ResolveInfo.Registration == registration);
        }
    }
}
