using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer.Diagnostics
{
    public sealed class DiagnosticsCollector
    {
        public string ScopeName { get; }

        readonly List<DiagnosticsInfo> diagnosticsInfos = new List<DiagnosticsInfo>();
        readonly Stack<DiagnosticsInfo> resolveCallStack = new Stack<DiagnosticsInfo>();

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

        public object TraceResolve(IRegistration registration, Func<IRegistration, object> resolving)
        {
            var current = DiagnositcsContext.FindByRegistration(registration);
            resolveCallStack.TryPeek(out var owner);

            if (!(registration is CollectionRegistration) && current != null && current != owner)
            {
                current.ResolveInfo.RefCount += 1;
                current.ResolveInfo.MaxDepth = current.ResolveInfo.MaxDepth < 0
                    ? resolveCallStack.Count
                    : Math.Max(current.ResolveInfo.MaxDepth, resolveCallStack.Count);

                owner?.Dependencies.Add(current);

                resolveCallStack.Push(current);
                var instance = resolving(registration);
                resolveCallStack.Pop();

                if (!current.ResolveInfo.Instances.Contains(instance))
                {
                    current.ResolveInfo.Instances.Add(instance);
                }

                return instance;
            }
            return resolving(registration);
        }
    }
}
