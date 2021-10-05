using System.Collections.Generic;

namespace VContainer.Diagnostics
{
    public sealed class DiagnosticsInfo
    {
        public ScopeKey ScopeKey { get; }
        public RegisterInfo RegisterInfo { get; }
        public ResolveInfo ResolveInfo { get; set; }
        public List<DiagnosticsInfo> Dependencies { get; } = new List<DiagnosticsInfo>();

        public DiagnosticsInfo(ScopeKey scopeKey, RegisterInfo registerInfo)
        {
            ScopeKey = scopeKey;
            RegisterInfo = registerInfo;
        }
    }
}
