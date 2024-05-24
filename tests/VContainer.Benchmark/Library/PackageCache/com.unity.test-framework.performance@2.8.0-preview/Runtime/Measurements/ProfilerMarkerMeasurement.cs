using System.Collections.Generic;
using Unity.PerformanceTesting.Runtime;
using UnityEngine;

namespace Unity.PerformanceTesting.Measurements
{
    internal class ProfilerMarkerMeasurement : MonoBehaviour
    {
        private bool m_MeasurementStarted;

        private readonly List<SampleGroup> m_SampleGroups = new List<SampleGroup>();

        public void AddProfilerSampleGroup(IEnumerable<SampleGroup> sampleGroups)
        {
            foreach (var sampleGroup in sampleGroups)
            {
                AddProfilerSample(sampleGroup);
            }
        }

        private void AddProfilerSample(SampleGroup sampleGroup)
        {
            sampleGroup.GetRecorder();
            sampleGroup.Recorder.enabled = true;
            m_SampleGroups.Add(sampleGroup);
        }

        private void SampleProfilerSamples()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                var delta = Utils.ConvertSample(SampleUnit.Nanosecond, sampleGroup.Unit, sampleGroup.Recorder.elapsedNanoseconds);
                Measure.Custom(sampleGroup, delta);
            }
        }

        public void StopAndSampleRecorders()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.Recorder.enabled = false;
                var delta = Utils.ConvertSample(SampleUnit.Nanosecond, sampleGroup.Unit, sampleGroup.Recorder.elapsedNanoseconds);
                Measure.Custom(sampleGroup, delta);
            }
        }

        public void Update()
        {
            if (!m_MeasurementStarted)
            {
                m_MeasurementStarted = true;
            }
            else
            {
                SampleProfilerSamples();
            }
        }
    }
}
