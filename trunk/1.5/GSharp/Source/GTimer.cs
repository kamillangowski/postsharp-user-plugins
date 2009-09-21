#region Released to Public Domain by eSymmetrix, Inc.

/********************************************************************************
 *   This file is sample code demonstrating Gibraltar integration with PostSharp
 *   
 *   This sample is free software: you have an unlimited rights to
 *   redistribute it and/or modify it.
 *   
 *   This sample is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *   
 *******************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection;
using Gibraltar.Agent.Metrics;
using PostSharp.Laos;

#endregion File Header

namespace Gibraltar.GSharp
{
    /// <summary>
    /// GTimer is a PostSharp aspect that will log execution time for methods.
    /// Data is stored as a Gibraltar metric allowing charting and graphing
    /// in Gibraltar Analyst.
    /// </summary>
    [Serializable]
    public sealed class GTimer : GAspectBase
    {
        // Tracing enabled by default, but can be enabled/disabled at run-time
        private static bool _enabled = true;

        /// <summary>
        /// Enables or disables tracing.  Note that tracing is enabled by default.
        /// </summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        // This local variable ensures that we log exit if we logged entry
        [NonSerialized]
        private bool _tracingEnabled;

        private string _metricCategory;
        private string _namespace;
        private const string MetricCategory = "PostSharp";
        [NonSerialized]
        private Stopwatch _stopwatch;

        public override void CompileTimeInitialize(MethodBase method)
        {
            base.CompileTimeInitialize(method);
            _metricCategory = MetricCategory + "." + ClassName;
            _namespace = method.DeclaringType.Namespace;
        }

        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);
            if (_stopwatch == null)
                _stopwatch = new Stopwatch();
        }

        public override void OnEntry(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            _tracingEnabled = Enabled;
            if (!_tracingEnabled)
                return;

            _stopwatch.Reset();
            _stopwatch.Start();
        }

        /// <summary>
        /// For convenience in Analyst, each duration is stored in two metrics.
        /// An overall metric is useful for charting hotspots grouping by method.
        /// Individual metrics per method are useful for graphing duration over time.
        /// </summary>
        public override void OnSuccess(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            if (!_tracingEnabled)
                return;

            _stopwatch.Stop();
            TimeSpan duration = _stopwatch.Elapsed;
            CreateSample(GetMetric("All Methods", MetricCategory, null), duration);
            CreateSample(GetMetric(MethodName, _metricCategory, null), duration);
        }

        /// <summary>
        /// Helper method to store the metric data
        /// </summary>
        private void CreateSample(EventMetric metric, TimeSpan duration)
        {
            EventMetricSample sample = metric.CreateSample();
            sample.SetValue("duration", duration);
            sample.SetValue("namespace", _namespace);
            sample.SetValue("class", ClassName);
            sample.SetValue("method", MethodName);
            sample.SetValue("fullname", _namespace + "." + QualifiedMethodName);
            sample.Write();
        }

        /// <summary>
        /// Helper function to retrieve the desired EventMetric
        /// </summary>
        private static EventMetric GetMetric(string caption, string category, string instance)
        {
            EventMetricDefinition cacheMetric;

            //so we can be called multiple times we want to see if the definition already exists.
            if (EventMetricDefinition.TryGetValue("PostSharp", category, caption, out cacheMetric) == false)
            {
                cacheMetric = new EventMetricDefinition("PostSharp", category, caption);

                //add the values (that are part of the definition)
                cacheMetric.DefaultValue = cacheMetric.AddValue("duration", typeof(TimeSpan), SummaryFunction.Average,
                                                                "ms", "Duration", "Average execution duration");
                cacheMetric.AddValue("namespace", typeof(string), SummaryFunction.Count, "", "Namespace", "Namespace");
                cacheMetric.AddValue("class", typeof(string), SummaryFunction.Count, "", "Class",
                                     "Class name ignoring namespace");
                cacheMetric.AddValue("method", typeof(string), SummaryFunction.Count, "", "Method",
                                     "Method name ignoring class and namespace");
                cacheMetric.AddValue("fullname", typeof(string), SummaryFunction.Count, "", "FullName",
                                     "Fully qualified name of the class and method");
                EventMetricDefinition.Register(ref cacheMetric);
            }

            EventMetric cacheEventMetric = EventMetric.Register(cacheMetric, instance);
            return cacheEventMetric;
        }
    }
}