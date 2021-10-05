using System;
using System.Collections.Generic;

namespace VContainer.Diagnostics
{
    public readonly struct ResolveTracingScope : IDisposable
    {
        [ThreadStatic]
        static int callStackCount;

        [ThreadStatic]
        static DiagnosticsInfo owner;

        public readonly DiagnosticsCollector Collector;
        public readonly IRegistration Registration;

        public ResolveTracingScope(DiagnosticsCollector collector, IRegistration registration)
        {
            Collector = collector;
            Registration = registration;

            callStackCount++;

            var current = collector.FindByRegistration(registration);
            if (current == null) return;

            current.ResolveInfo.ResolveCount += 1;

            if (callStackCount > 1)
            {
                owner?.Dependencies.Add(current);
            }
            owner = current;
        }

        public void Dispose()
        {
            if (--callStackCount <= 0)
            {
                callStackCount = 0;
                owner = null;
            }
        }
    }

    public sealed class DiagnosticsCollector
    {
        public ScopeKey ScopeKey { get; }

        readonly List<DiagnosticsInfo> diagnosticsInfos = new List<DiagnosticsInfo>();

        public DiagnosticsCollector(ScopeKey scopeKey)
        {
            ScopeKey = scopeKey;
        }

        public IReadOnlyList<DiagnosticsInfo> GetDiagnosticsInfos()
        {
            return diagnosticsInfos;
        }

        public void Clear()
        {
            lock (diagnosticsInfos)
            {
                diagnosticsInfos.Clear();
            }
        }

        public void TraceRegister(RegisterInfo registerInfo)
        {
            lock (diagnosticsInfos)
            {
                diagnosticsInfos.Add(new DiagnosticsInfo(ScopeKey, registerInfo));
            }
        }

        public void TraceBuild(RegistrationBuilder registrationBuilder, IRegistration registration)
        {
            lock (diagnosticsInfos)
            {
                foreach (var x in diagnosticsInfos)
                {
                    if (x.RegisterInfo.RegistrationBuilder == registrationBuilder)
                    {
                        x.ResolveInfo = new ResolveInfo(registration);
                        return;
                    }
                }
            }
        }

        public ResolveTracingScope StartResolveTracing(IRegistration registration)
        {
            return new ResolveTracingScope(this, registration);
        }

        internal DiagnosticsInfo FindByRegistration(IRegistration registration)
        {
            lock (diagnosticsInfos)
            {
                foreach (var x in diagnosticsInfos)
                {
                    if (x.ResolveInfo.Registration == registration)
                    {
                        return x;
                    }
                }
            }
            return null;
        }
    }
}
