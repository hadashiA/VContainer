using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VContainer.Diagnostics
{
    public interface IDiagnosticsCollector
    {
        ILookup<string, DiagnosticsInfo> GetDiagnosticsInfos();
        ILookup<string, DiagnosticsInfo> GetGroupedDiagnosticsInfos();

        void Clear(IContainerBuilder containerBuilder);

        void TraceRegister(
            IContainerBuilder containerBuilder,
            RegistrationBuilder registrationBuilder,
            StackTrace stackTrace);

        void TraceBuild(
            IContainerBuilder containerBuilder,
            RegistrationBuilder registrationBuilder,
            IRegistration registration);
    }

    public sealed class DiagnosticsInfo
    {
        public string ScopeName { get; }
        public RegisterInfo RegisterInfo { get; }
        public IRegistration Registration { get; set; }
        public readonly List<ResolveInfo> Resolves = new List<ResolveInfo>();

        public DiagnosticsInfo(string scopeName, RegisterInfo registerInfo)
        {
            ScopeName = scopeName;
            RegisterInfo = registerInfo;
        }
    }

    public sealed class DiagnosticsCollector : IDiagnosticsCollector
    {
        static string GetScopeName(IContainerBuilder containerBuilder)
        {
            if (containerBuilder.ApplicationOrigin is UnityEngine.Object obj)
            {
                return obj.name;
            }

            var typeName = containerBuilder.GetType().Name;
            var suffixIndex = typeName.IndexOf("Builder");
            return suffixIndex > 0 ? typeName.Substring(0, suffixIndex) : typeName;
        }

        readonly Dictionary<string, Dictionary<RegistrationBuilder, DiagnosticsInfo>> diagnosticsInfos =
            new Dictionary<string, Dictionary<RegistrationBuilder, DiagnosticsInfo>>();

        readonly object syncRoot = new object();

        public void Clear(IContainerBuilder containerBuilder)
        {
            var scopeName = GetScopeName(containerBuilder);
            lock (syncRoot)
            {
                if (diagnosticsInfos.TryGetValue(scopeName, out var entry))
                {
                    entry.Clear();
                }
            }
        }

        public void TraceRegister(
            IContainerBuilder containerBuilder,
            RegistrationBuilder registrationBuilder,
            StackTrace stackTrace)
        {
            var scopeName = GetScopeName(containerBuilder);
            lock (syncRoot)
            {
                if (!diagnosticsInfos.TryGetValue(scopeName, out var entry))
                {
                    entry = new Dictionary<RegistrationBuilder, DiagnosticsInfo>();
                    diagnosticsInfos.Add(scopeName, entry);
                }
                entry.Add(registrationBuilder, new DiagnosticsInfo(scopeName, new RegisterInfo(registrationBuilder, stackTrace)));
            }
        }

        public void TraceBuild(
            IContainerBuilder containerBuilder,
            RegistrationBuilder registrationBuilder,
            IRegistration registration)
        {
            var scopeName = GetScopeName(containerBuilder);
            lock (syncRoot)
            {
                if (!diagnosticsInfos.TryGetValue(scopeName, out var entry))
                {
                    entry = new Dictionary<RegistrationBuilder, DiagnosticsInfo>();
                    diagnosticsInfos.Add(scopeName, entry);
                }

                if (entry.TryGetValue(registrationBuilder, out var info))
                {
                    info.Registration = registration;
                }
            }
        }

        public void TraceResolve(IObjectResolver container, IRegistration registration, object instance)
        {
            lock (syncRoot)
            {
                throw new NotImplementedException();
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
