using VContainer.Diagnostics;

namespace VContainer.Unity
{
    partial class LifetimeScope
    {
        public static bool DiagnosticsEnabled
            // => VContainerSettings.Instance != null && VContainerSettings.Instance.EnableDiagnostics;
            => true; // TODO:
    }
}
