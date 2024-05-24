using System;
using System.Collections.Generic;
using Unity.PerformanceTesting.Exceptions;
using UnityEngine.Profiling;

namespace Unity.PerformanceTesting
{
    [Serializable]
    public class SampleGroup
    {
        public string Name;
        public SampleUnit Unit;
        public bool IncreaseIsBetter;
        public List<double> Samples = new List<double>();
        public double Min;
        public double Max;
        public double Median;
        public double Average;
        public double StandardDeviation;
        public double Sum;

        public SampleGroup(string name, SampleUnit unit = SampleUnit.Millisecond, bool increaseIsBetter = false)
        {
            Name = name;
            Unit = unit;
            IncreaseIsBetter = increaseIsBetter;

            if (string.IsNullOrEmpty(name))
            {
                throw new PerformanceTestException("Sample group name is empty. Please assign a valid name.");
            }
        }
        
        internal Recorder Recorder;

        public Recorder GetRecorder()
        {
            return Recorder ?? (Recorder = Recorder.Get(Name));
        }
    }
}
