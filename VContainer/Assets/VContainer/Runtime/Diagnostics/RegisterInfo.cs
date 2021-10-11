using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;

namespace VContainer.Diagnostics
{
    public sealed class RegisterInfo
    {
        static bool displayFileNames = true;
        static int idSeed;

        public int Id { get; }
        public RegistrationBuilder RegistrationBuilder { get; }
        public StackTrace StackTrace { get; }

        StackFrame headLineStackFrame;

        internal string formattedStackTrace = default; // cache field for internal use(Unity Editor, etc...)

        public RegisterInfo(RegistrationBuilder registrationBuilder)
        {
            Id = Interlocked.Increment(ref idSeed);
            RegistrationBuilder = registrationBuilder;
            StackTrace = new StackTrace(true);
            headLineStackFrame = GetHeadlineFrame(StackTrace);
        }

        public string GetFilename()
        {
            if (headLineStackFrame != null && displayFileNames && headLineStackFrame.GetILOffset() != -1)
            {
                try
                {
                    return headLineStackFrame.GetFileName();
                }
                catch (NotSupportedException)
                {
                    displayFileNames = false;
                }
                catch (SecurityException)
                {
                    displayFileNames = false;
                }
            }
            return null;
        }

        public int GetFileLineNumber()
        {
            if (headLineStackFrame != null && displayFileNames && headLineStackFrame.GetILOffset() != -1)
            {
                try
                {
                    return headLineStackFrame.GetFileLineNumber();
                }
                catch (NotSupportedException)
                {
                    displayFileNames = false;
                }
                catch (SecurityException)
                {
                    displayFileNames = false;
                }
            }
            return -1;
        }

        public string GetScriptAssetPath()
        {
            var filename = GetFilename();
            if (filename == null)
                return "";
            var prefixIndex = filename.LastIndexOf("Assets/");
            return prefixIndex > 0 ? filename.Substring(prefixIndex) : "";
        }

        public string GetHeadline()
        {
            if (headLineStackFrame == null)
                return "";

            var method = headLineStackFrame.GetMethod();
            var filename = GetFilename();
            if (filename != null)
            {
                var lineNumber = GetFileLineNumber();
                return $"{method.DeclaringType?.FullName}.{method.Name} (at {Path.GetFileName(filename)}:{lineNumber})";
            }

            var ilOffset = headLineStackFrame.GetILOffset();
            if (ilOffset != -1)
            {
                return $"{method.DeclaringType?.FullName}.{method.Name}(offset: {ilOffset})";
            }
            return $"{method.DeclaringType?.FullName}.{method.Name}";
        }

        StackFrame GetHeadlineFrame(StackTrace stackTrace)
        {
            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var sf = stackTrace.GetFrame(i);
                if (sf == null) continue;

                var m = sf.GetMethod();
                if (m == null) continue;

                if (m.DeclaringType == null) continue;
                if (m.DeclaringType.Namespace == null || !m.DeclaringType.Namespace.StartsWith("VContainer"))
                {
                    return sf;
                }
            }
            return stackTrace.FrameCount > 0 ? stackTrace.GetFrame(0) : null;
        }
    }
}