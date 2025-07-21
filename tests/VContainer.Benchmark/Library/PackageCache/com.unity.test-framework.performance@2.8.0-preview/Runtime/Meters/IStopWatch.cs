namespace Unity.PerformanceTesting.Meters
{
    /// <summary>
    /// Provides stopwatch functionality for measuring time
    /// </summary>
    internal interface IStopWatch
    {
        /// <summary>
        /// Resets and starts the stopwatch
        /// </summary>
        void Start();

        /// <summary>
        /// Takes a split time since start of stopwatch
        /// </summary>
        /// <returns>Time passed since start in milliseconds</returns>
        double Split();
    }
}