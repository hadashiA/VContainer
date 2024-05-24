using UnityEngine;

namespace Unity.PerformanceTesting.Measurements
{
    internal class FrameTimeMeasurement : MonoBehaviour
    {
        public SampleGroup SampleGroup;

        void Update()
        {
            Measure.Custom(SampleGroup, Time.unscaledDeltaTime * 1000);
        }
    }
}
