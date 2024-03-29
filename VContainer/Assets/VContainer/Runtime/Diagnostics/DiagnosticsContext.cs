using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Diagnostics
{
    public static class DiagnositcsContext
    {
        static readonly Dictionary<int, DiagnosticsCollector> collectors
            = new Dictionary<int, DiagnosticsCollector>();

        public static event Action<IObjectResolver> OnContainerBuilt;

        public static DiagnosticsCollector GetCollector(int instanceId, string name)
        {
            lock (collectors)
            {
                if (!collectors.TryGetValue(instanceId, out var collector))
                {
                    collector = new DiagnosticsCollector($"${name}/{instanceId}");
                    collectors.Add(instanceId, collector);
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

        public static void NotifyContainerBuilt(IObjectResolver container)
        {
            OnContainerBuilt?.Invoke(container);
        }

        internal static DiagnosticsInfo FindByRegistration(Registration registration)
        {
            return GetDiagnosticsInfos().FirstOrDefault(x => x.ResolveInfo.Registration == registration);
        }

        public static void RemoveCollector(int instanceId)
        {
            lock (collectors)
            {
                collectors.Remove(instanceId);
            }
        }
    }
}
