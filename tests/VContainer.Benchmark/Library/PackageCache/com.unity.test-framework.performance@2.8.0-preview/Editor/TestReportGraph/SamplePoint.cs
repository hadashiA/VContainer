using System;

namespace Unity.PerformanceTesting.Editor
{
    struct SamplePoint : IComparable<SamplePoint>
    {
        public double sample;
        public int index;

        public SamplePoint(double _sample, int _index)
        {
            sample = _sample;
            index = _index;
        }

        public int CompareTo(SamplePoint other)
        {
            return sample.CompareTo(other.sample);
        }
    }
}
