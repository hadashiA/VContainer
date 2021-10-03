using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Diagnostics
{
    public interface IDiagnosticsCollector
    {
        ILookup<string, DiagnosticsInfo> GetDiagnosticsInfos();
        ILookup<string, DiagnosticsInfo> GetGroupedDiagnosticsInfos();
        void Collect(IObjectResolver container, IRegistration[] registrations);
    }

    public sealed class DiagnosticsInfo
    {
        public readonly string ScopeName;
        public readonly IRegistration Registration;
        public readonly ResolveInfo ResolveInfo;
        public List<ResolveInfo> Resolves = new List<ResolveInfo>();

        public DiagnosticsInfo(string scopeName, IRegistration registration)
        {
            ScopeName = scopeName;
            Registration = registration;
        }
    }

    public sealed class DiagnosticsCollector : IDiagnosticsCollector
    {
        static string GetScopeName(IObjectResolver container)
        {
            if (container.ApplicationOrigin is UnityEngine.Object obj)
            {
                return obj.name;
            }

            return container.GetType().Name;
        }

        readonly Dictionary<string, Dictionary<IRegistration, DiagnosticsInfo>> diagnosticsInfos =
            new Dictionary<string, Dictionary<IRegistration, DiagnosticsInfo>>();

        readonly object syncRoot = new object();

        public void AddInstance(IObjectResolver container, IRegistration registration, object instance)
        {
            lock (syncRoot)
            {
                throw new NotImplementedException();
            }
        }

        public void Collect(IObjectResolver container, IRegistration[] registrations)
        {
            var scopeName = GetScopeName(container);

            lock (syncRoot)
            {
                if (diagnosticsInfos.TryGetValue(scopeName, out var entry))
                {
                    entry.Clear();
                }
                else
                {
                    entry = new Dictionary<IRegistration, DiagnosticsInfo>();
                    diagnosticsInfos.Add(scopeName, entry);
                }

                foreach (var registration in registrations)
                {
                    entry.Add(registration, new DiagnosticsInfo(scopeName, registration));
                }
            }
        }

        public ILookup<string, DiagnosticsInfo> GetDiagnosticsInfos()
        {
            lock (syncRoot)
            {
                throw new NotImplementedException();
                // return diagnosticsInfos.SelectMany(x => x.Value.Values).ToArray();
            }
        }

        public ILookup<string, DiagnosticsInfo> GetGroupedDiagnosticsInfos()
        {
            lock (syncRoot)
            {
                return diagnosticsInfos
                    .SelectMany(x => x.Value.Values)
                    .ToLookup(x => x.ScopeName);
            }
        }
    }
}
