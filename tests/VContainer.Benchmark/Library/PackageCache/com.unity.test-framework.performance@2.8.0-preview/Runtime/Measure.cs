using System;
using System.Diagnostics;
using Unity.PerformanceTesting.Exceptions;
using Unity.PerformanceTesting.Measurements;
using Unity.PerformanceTesting.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.PerformanceTesting
{
    public static class Measure
    {
        public static void Custom(SampleGroup sampleGroup, double value)
        {
            VerifyValue(sampleGroup.Name, value);

            var activeSampleGroup = PerformanceTest.GetSampleGroup(sampleGroup.Name);
            if (activeSampleGroup == null)
            {
                PerformanceTest.AddSampleGroup(sampleGroup);
                activeSampleGroup = sampleGroup;
            }

            activeSampleGroup.Samples.Add(value);
        }

        public static void Custom(string name, double value)
        {
            VerifyValue(name, value);

            var activeSampleGroup = PerformanceTest.GetSampleGroup(name);
            if (activeSampleGroup == null)
            {
                activeSampleGroup = new SampleGroup(name);
                PerformanceTest.AddSampleGroup(activeSampleGroup);
            }

            activeSampleGroup.Samples.Add(value);
        }

        static void VerifyValue(string name, double value)
        {
            if (double.IsNaN(value))
                throw new PerformanceTestException(
                    $"Trying to record value which is not a number for sample group: {name}");
        }

        public static ScopeMeasurement Scope(string name = "Time")
        {
            return new ScopeMeasurement(name);
        }
        
        public static ScopeMeasurement Scope(SampleGroup sampleGroup)
        {
            return new ScopeMeasurement(sampleGroup);
        }

        public static ProfilerMeasurement ProfilerMarkers(params string[] profilerMarkerLabels)
        {
            return new ProfilerMeasurement(profilerMarkerLabels);
        }
        
        public static ProfilerMeasurement ProfilerMarkers(params SampleGroup[] sampleGroups)
        {
            return new ProfilerMeasurement(sampleGroups);
        }

        public static MethodMeasurement Method(Action action)
        {
            return new MethodMeasurement(action);
        }

        public static FramesMeasurement Frames()
        {
            return new FramesMeasurement();
        }
    }

    public struct ScopeMeasurement : IDisposable
    {
        private readonly SampleGroup m_SampleGroup;
        private readonly long m_StartTicks;

        public ScopeMeasurement(SampleGroup sampleGroup)
        {
            m_SampleGroup = PerformanceTest.GetSampleGroup(sampleGroup.Name);
            if (m_SampleGroup == null)
            {
                m_SampleGroup = sampleGroup;
                PerformanceTest.Active.SampleGroups.Add(m_SampleGroup);
            }

            m_StartTicks = Stopwatch.GetTimestamp();
            PerformanceTest.Disposables.Add(this);
        }

        public ScopeMeasurement(string name) : this(new SampleGroup(name))
        {
        }

        public void Dispose()
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - m_StartTicks;
            PerformanceTest.Disposables.Remove(this);
            var delta = TimeSpan.FromTicks(elapsedTicks).TotalMilliseconds;
            
            delta = Utils.ConvertSample(SampleUnit.Millisecond, m_SampleGroup.Unit, delta);

            Measure.Custom(m_SampleGroup, delta);
        }
    }

    public struct ProfilerMeasurement : IDisposable
    {
        private readonly ProfilerMarkerMeasurement m_Test;

        public ProfilerMeasurement(SampleGroup[] sampleGroups)
        {
            if (sampleGroups == null)
            {
                m_Test = null;
                return;
            }

            if (sampleGroups.Length == 0)
            {
                m_Test = null;
                return;
            }

            var go = new GameObject("Recorder");
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            m_Test = go.AddComponent<ProfilerMarkerMeasurement>();
            m_Test.AddProfilerSampleGroup(sampleGroups);
            PerformanceTest.Disposables.Add(this);
        }

        public ProfilerMeasurement(string[] profilerMarkers): this(Utils.CreateSampleGroupsFromMarkerNames(profilerMarkers))
        {
        }

        public void Dispose()
        {
            PerformanceTest.Disposables.Remove(this);
            if (m_Test == null) return;
            m_Test.StopAndSampleRecorders();
            Object.DestroyImmediate(m_Test.gameObject);
        }
    }
}