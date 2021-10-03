using System.Diagnostics;
using System.Threading;

namespace VContainer.Diagnostics
{
    public sealed class ResolveInfo
    {
        static int idSeed;

        public readonly int Id;
        public readonly object Instance;
        public readonly StackTrace StackTrace;

        internal string FormattedStackTrace = ""; // cache field for internal use(Unity Editor, etc...)

        public ResolveInfo(object instance, StackTrace stackTrace)
        {
            Id = Interlocked.Increment(ref idSeed);
            Instance = instance;
            StackTrace = stackTrace;
        }
   }
}