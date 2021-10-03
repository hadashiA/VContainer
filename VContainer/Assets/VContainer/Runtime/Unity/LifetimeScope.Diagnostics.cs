using VContainer.Diagnostics;

namespace VContainer.Unity
{
    partial class LifetimeScope
    {
        public static readonly IDiagnosticsCollector DiagnosticsCollector = new DiagnosticsCollector();

        public static bool DiagnosticsEnabled
            // => VContainerSettings.Instance != null && VContainerSettings.Instance.EnableDiagnostics;
            => true; // TODO:
    }
}
