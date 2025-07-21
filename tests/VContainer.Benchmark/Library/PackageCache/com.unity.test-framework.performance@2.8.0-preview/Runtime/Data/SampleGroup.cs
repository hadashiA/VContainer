using System;
using System.Collections.Generic;
using Unity.PerformanceTesting.Exceptions;

namespace Unity.PerformanceTesting.Data
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

        public SampleGroup(){}
        public SampleGroup(string name, SampleUnit unit, bool increaseIsBetter)
        {
            Name = name;
            Unit = unit;
            IncreaseIsBetter = increaseIsBetter;

            if (string.IsNullOrEmpty(name))
            {
                throw new PerformanceTestException("Sample group name is empty. Please assign a valid name.");
            }
        }
    }
}
