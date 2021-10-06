using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer.Diagnostics
{
    public readonly struct ResolveTracingScope : IDisposable
    {
        [ThreadStatic]
        static Stack<DiagnosticsInfo> dependencyStack;

        public readonly DiagnosticsCollector Collector;
        public readonly IRegistration Registration;
        public readonly bool Traced;

        public ResolveTracingScope(DiagnosticsCollector collector, IRegistration registration)
        {
            Collector = collector;
            Registration = registration;
            Traced = !(registration is CollectionRegistration);

            if (!Traced)
                return;

            if (dependencyStack == null)
                dependencyStack = new Stack<DiagnosticsInfo>();

            dependencyStack.TryPeek(out var owner);

            var current = collector.FindByRegistration(registration);
            dependencyStack.Push(current);

            if (current == null || owner == current)
                return;
            current.ResolveInfo.RefCount += 1;

            current.ResolveInfo.MaxDepth = current.ResolveInfo.MaxDepth < 0
                ? dependencyStack.Count
                : Math.Max(current.ResolveInfo.MaxDepth, dependencyStack.Count);

            owner?.Dependencies.Add(current);
        }

        public void Dispose()
        {
            if (dependencyStack.Count > 0 && Traced)
                dependencyStack.Pop();
        }
    }

    public sealed class DiagnosticsCollector
    {
        public string ScopeName { get; }

        readonly List<DiagnosticsInfo> diagnosticsInfos = new List<DiagnosticsInfo>();

        public DiagnosticsCollector(string scopeName)
        {
            ScopeName = scopeName;
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
                diagnosticsInfos.Add(new DiagnosticsInfo(ScopeName, registerInfo));
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
