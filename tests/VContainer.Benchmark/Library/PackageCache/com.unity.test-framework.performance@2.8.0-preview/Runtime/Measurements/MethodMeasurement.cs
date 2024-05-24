using System;
using System.Collections.Generic;
using Unity.PerformanceTesting.Runtime;
using Unity.PerformanceTesting.Exceptions;
using Unity.PerformanceTesting.Meters;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.PerformanceTesting.Measurements
{
    public class MethodMeasurement
    {
        internal const int k_MeasurementCount = 9;
        private const int k_MinMeasurementTimeMs = 100;
        private const int k_MinWarmupTimeMs = 100;
        private const int k_ProbingMultiplier = 4;
        private const int k_MaxIterations = 10000;
        private readonly Action m_Action;
        private readonly List<SampleGroup> m_SampleGroups = new List<SampleGroup>();
        private readonly Recorder m_GCRecorder;

        private Action m_Setup;
        private Action m_Cleanup;
        private SampleGroup m_SampleGroup = new SampleGroup("Time", SampleUnit.Millisecond, false);
        private SampleGroup m_SampleGroupGC = new SampleGroup("Time.GC()", SampleUnit.Undefined, false);
        private int m_WarmupCount;
        private int m_MeasurementCount;
        private int m_IterationCount = 1;
        private bool m_GC;
        private IStopWatch m_Watch;

        public MethodMeasurement(Action action)
        {
            m_Action = action;
            m_GCRecorder = Recorder.Get("GC.Alloc");
            m_GCRecorder.enabled = false;
            if (m_Watch == null) m_Watch = new StopWatch();
        }

        internal MethodMeasurement StopWatch(IStopWatch watch)
        {
            m_Watch = watch;

            return this;
        }

        public MethodMeasurement ProfilerMarkers(params string[] profilerMarkerNames)
        {
            if (profilerMarkerNames == null) return this;
            foreach (var marker in profilerMarkerNames)
            {
                var sampleGroup = new SampleGroup(marker, SampleUnit.Nanosecond, false);
                sampleGroup.GetRecorder();
                sampleGroup.Recorder.enabled = false;
                m_SampleGroups.Add(sampleGroup);
            }

            return this;
        }
        
        public MethodMeasurement ProfilerMarkers(params SampleGroup[] sampleGroups)
        {
            if (sampleGroups == null){ return this;}
            foreach (var sampleGroup in sampleGroups)
            {
                sampleGroup.GetRecorder();
                sampleGroup.Recorder.enabled = false;
                m_SampleGroups.Add(sampleGroup);
            }

            return this;
        }

        public MethodMeasurement SampleGroup(string name)
        {
            m_SampleGroup = new SampleGroup(name, SampleUnit.Millisecond, false);
            m_SampleGroupGC = new SampleGroup(name + ".GC()", SampleUnit.Undefined, false);
            return this;
        }
        
        public MethodMeasurement SampleGroup(SampleGroup sampleGroup)
        {
            m_SampleGroup = sampleGroup;
            m_SampleGroupGC = new SampleGroup(sampleGroup.Name + ".GC()", SampleUnit.Undefined, false);
            return this;
        }

        public MethodMeasurement WarmupCount(int count)
        {
            m_WarmupCount = count;
            return this;
        }

        public MethodMeasurement IterationsPerMeasurement(int count)
        {
            m_IterationCount = count;
            return this;
        }

        public MethodMeasurement MeasurementCount(int count)
        {
            m_MeasurementCount = count;
            return this;
        }

        public MethodMeasurement CleanUp(Action action)
        {
            m_Cleanup = action;
            return this;
        }

        public MethodMeasurement SetUp(Action action)
        {
            m_Setup = action;
            return this;
        }

        public MethodMeasurement GC()
        {
            m_GC = true;
            return this;
        }

        public void Run()
        {
            if (m_MeasurementCount > 0)
            {
                Warmup(m_WarmupCount);
                RunForIterations(m_IterationCount, m_MeasurementCount, useAverage: false);
                return;
            }

            var iterations = Probing();
            RunForIterations(iterations, k_MeasurementCount, useAverage: true);
        }

        private void RunForIterations(int iterations, int measurements, bool useAverage)
        {
            EnableMarkers();
            for (var j = 0; j < measurements; j++)
            {
                var executionTime = iterations == 1 ? ExecuteSingleIteration() : ExecuteForIterations(iterations);
                if (useAverage) executionTime /= iterations;
                var delta = Utils.ConvertSample(SampleUnit.Millisecond, m_SampleGroup.Unit, executionTime);
                Measure.Custom(m_SampleGroup, delta);
            }

            DisableAndMeasureMarkers();
        }

        private void EnableMarkers()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.Recorder.enabled = true;
            }
        }

        private void DisableAndMeasureMarkers()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.Recorder.enabled = false;
                var sample = sampleGroup.Recorder.elapsedNanoseconds;
                var blockCount = sampleGroup.Recorder.sampleBlockCount;
                if(blockCount == 0) continue;
                var delta = Utils.ConvertSample(SampleUnit.Nanosecond, sampleGroup.Unit, sample);
                Measure.Custom(sampleGroup, delta / blockCount);
            }
        }

        private int Probing()
        {
            var executionTime = 0.0D;
            var iterations = 1;

            if (m_WarmupCount > 0)
                throw new PerformanceTestException(
                    "Please provide MeasurementCount or remove WarmupCount in your usage of Measure.Method");

            while (executionTime < k_MinWarmupTimeMs)
            {
                executionTime = m_Watch.Split();
                Warmup(iterations);
                executionTime = m_Watch.Split() - executionTime;

                if (executionTime < k_MinWarmupTimeMs)
                {
                    iterations *= k_ProbingMultiplier;
                }
            }

            if (iterations == 1)
            {
                ExecuteActionWithCleanupSetup();
                ExecuteActionWithCleanupSetup();

                return 1;
            }

            var deisredIterationsCount =
                Mathf.Clamp((int) (k_MinMeasurementTimeMs * iterations / executionTime), 1, k_MaxIterations);

            return deisredIterationsCount;
        }

        private void Warmup(int iterations)
        {
            for (var i = 0; i < iterations; i++)
            {
                ExecuteForIterations(m_IterationCount);
            }
        }

        private double ExecuteActionWithCleanupSetup()
        {
            m_Setup?.Invoke();

            var executionTime = m_Watch.Split();
            m_Action.Invoke();
            executionTime = m_Watch.Split() - executionTime;

            m_Cleanup?.Invoke();

            return executionTime;
        }

        private double ExecuteSingleIteration()
        {
            if (m_GC) StartGCRecorder();
            m_Setup?.Invoke();

            var executionTime = m_Watch.Split();
            m_Action.Invoke();
            executionTime = m_Watch.Split() - executionTime;

            m_Cleanup?.Invoke();
            if (m_GC) EndGCRecorderAndMeasure(1);
            return executionTime;
        }

        private double ExecuteForIterations(int iterations)
        {
            if (m_GC) StartGCRecorder();
            var executionTime = 0.0D;

            if (m_Cleanup != null || m_Setup != null)
            {
                for (var i = 0; i < iterations; i++)
                {
                    executionTime += ExecuteActionWithCleanupSetup();
                }
            }
            else
            {
                executionTime = m_Watch.Split();
                for (var i = 0; i < iterations; i++)
                {
                    m_Action.Invoke();
                }

                executionTime = m_Watch.Split() - executionTime;
            }

            if (m_GC) EndGCRecorderAndMeasure(iterations);
            return executionTime;
        }

        private void StartGCRecorder()
        {
            System.GC.Collect();

            m_GCRecorder.enabled = false;
            m_GCRecorder.enabled = true;
        }

        private void EndGCRecorderAndMeasure(int iterations)
        {
            m_GCRecorder.enabled = false;

            Measure.Custom(m_SampleGroupGC, (double) m_GCRecorder.sampleBlockCount / iterations);
        }
    }
}
