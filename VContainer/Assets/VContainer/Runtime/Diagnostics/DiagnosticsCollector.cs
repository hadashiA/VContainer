using System;
using System.Collections.Generic;
using System.Linq;
using VContainer.Unity;

namespace VContainer.Diagnostics
{
    public interface IDiagnosticsCollector
    {
        ILookup<IObjectResolver, DiagnosticsInfo> GetDiagnosticsInfos();
        ILookup<IObjectResolver, DiagnosticsInfo> GetGroupedDiagnosticsInfos();
    }

    public sealed class DiagnosticsInfo
    {
        public readonly IObjectResolver Container;
        public readonly IRegistration Registration;
        public readonly ResolveInfo ResolveInfo;
        public List<ResolveInfo> Resolves = new List<ResolveInfo>();

        public DiagnosticsInfo(
            IObjectResolver container,
            IRegistration registration,
            ResolveInfo resolveInfo = null)
        {
            Container = container;
            Registration = registration;
            ResolveInfo = resolveInfo;
        }
    }

    public sealed class DiagnosticsCollector : IDiagnosticsCollector
    {
        readonly Dictionary<IObjectResolver, Dictionary<IRegistration, DiagnosticsInfo>> diagnosticsInfos =
            new Dictionary<IObjectResolver, Dictionary<IRegistration, DiagnosticsInfo>>();

        readonly object syncRoot = new object();

        public void AddRegistration(IObjectResolver container, IRegistration registration)
        {
            lock (syncRoot)
            {
                if (diagnosticsInfos.TryGetValue(container, out var dict))
                {
                    dict.Add(registration, new DiagnosticsInfo(container, registration));
                }
                else
                {
                    diagnosticsInfos.Add(container, new Dictionary<IRegistration, DiagnosticsInfo>());
                }
            }
        }

        public void AddInstance(IObjectResolver container, IRegistration registration, object instance)
        {
            lock (syncRoot)
            {
                throw new NotImplementedException();
            }
        }

        public ILookup<IObjectResolver, DiagnosticsInfo> GetDiagnosticsInfos()
        {
            lock (syncRoot)
            {
                throw new NotImplementedException();
                // return diagnosticsInfos.SelectMany(x => x.Value.Values).ToArray();
            }
        }

        public ILookup<IObjectResolver, DiagnosticsInfo> GetGroupedDiagnosticsInfos()
        {
            lock (syncRoot)
            {
                return diagnosticsInfos
                    .SelectMany(x => x.Value.Values)
                    .ToLookup(x => x.Container);
            }
        }
    }
}
