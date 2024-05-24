using System.Diagnostics;

namespace Unity.PerformanceTesting.Meters
{
    /// <summary>
    /// Takes use of System.Diagnostics.Stopwatch to provide stopwatch functionality implementing IStopWatch
    /// </summary>
    internal class StopWatch : IStopWatch
    {
        private readonly Stopwatch m_StopWatch = Stopwatch.StartNew();
        
        public void Start()
        {
            m_StopWatch.Restart();
        }

        public double Split()
        {
            return m_StopWatch.Elapsed.TotalMilliseconds;
        }
    }
}