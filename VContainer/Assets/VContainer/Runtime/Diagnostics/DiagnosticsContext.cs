using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Diagnostics
{
    public readonly struct ScopeKey : IEquatable<ScopeKey>
    {
        public readonly string Name;
        public readonly int InstanceId;

        public ScopeKey(string name, int instanceId)
        {
            Name = name;
            InstanceId = instanceId;
        }

        public bool Equals(ScopeKey other) => Name == other.Name && InstanceId == other.InstanceId;
        public override bool Equals(object obj) => obj is ScopeKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Name, InstanceId);

        public override string ToString() => $"{Name} {InstanceId}";
    }

    public static class DiagnositcsContext
    {
        static readonly Dictionary<ScopeKey, DiagnosticsCollector> collectors
            = new Dictionary<ScopeKey, DiagnosticsCollector>();

        public static DiagnosticsCollector GetCurrentCollector(string name, int instanceId)
        {
            var scopeKey = new ScopeKey(name, instanceId);
            lock (collectors)
            {
                if (!collectors.TryGetValue(scopeKey, out var collector))
                {
                    collector = new DiagnosticsCollector(scopeKey);
                    collectors.Add(scopeKey, collector);
                }
                return collector;
            }
        }

        public static ILookup<ScopeKey, DiagnosticsInfo> GetGroupedDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors
                    .SelectMany(x => x.Value.GetDiagnosticsInfos())
                    .ToLookup(x => x.ScopeKey);
            }
        }

        public static IEnumerable<DiagnosticsInfo> GetDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors.SelectMany(x => x.Value.GetDiagnosticsInfos());
            }
        }
    }
}