using System;
using System.Collections.Generic;
using System.Threading;
using VContainer.Internal;

namespace VContainer.Diagnostics
{
    public sealed class DiagnosticsCollector
    {
        public string ScopeName { get; }

        readonly List<DiagnosticsInfo> diagnosticsInfos = new List<DiagnosticsInfo>();
        readonly ThreadLocal<Stack<DiagnosticsInfo>> resolveCallStack
            = new ThreadLocal<Stack<DiagnosticsInfo>>(() => new Stack<DiagnosticsInfo>());

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

        public void TraceBuild(RegistrationBuilder registrationBuilder, Registration registration)
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

        public object TraceResolve(Registration registration, Func<Registration, object> resolving)
        {
            var current = DiagnositcsContext.FindByRegistration(registration);
            var owner = resolveCallStack.Value.Count > 0 ? resolveCallStack.Value.Peek() : null;

            if (!(registration.Provider is CollectionInstanceProvider) && current != null && current != owner)
            {
                current.ResolveInfo.RefCount += 1;
                current.ResolveInfo.MaxDepth = current.ResolveInfo.MaxDepth < 0
                    ? resolveCallStack.Value.Count
                    : Math.Max(current.ResolveInfo.MaxDepth, resolveCallStack.Value.Count);

                owner?.Dependencies.Add(current);

                resolveCallStack.Value.Push(current);
                var instance = resolving(registration);
                resolveCallStack.Value.Pop();

                if (!current.ResolveInfo.Instances.Contains(instance))
                {
                    current.ResolveInfo.Instances.Add(instance);
                }

                return instance;
            }
            return resolving(registration);
        }

        public void NotifyContainerBuilt(IObjectResolver container)
            => DiagnositcsContext.NotifyContainerBuilt(container);
    }
}
