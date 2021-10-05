using System.Collections.Generic;

namespace VContainer.Diagnostics
{
    public sealed class ResolveInfo
    {
        public IRegistration Registration { get; }
        public List<object> Instances { get; } = new List<object>();
        public int ResolveCount { get; set; }

        public ResolveInfo(IRegistration registration)
        {
            Registration = registration;
        }
    }
}
