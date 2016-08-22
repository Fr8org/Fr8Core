using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Core;
using System.IO;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace HealthMonitor
{
    public class NUnitTraceListener : EventListener, IDisposable
    {
        ConsoleTraceListener _listener = new ConsoleTraceListener();
        TelemetryClient _telemetry;

        public NUnitTraceListener(string appInsightsInstrumentationKey)
        {
            //Remove OWIN listeners to avoid multiple copies of message getting to console
            var listeners = Trace.Listeners.OfType<TextWriterTraceListener>().Where(l => l.Name == "HostingTraceListener").ToList();
            foreach (var l in listeners)
            {
                Trace.Listeners.Remove("HostingTraceListener");
            }

            Trace.Listeners.Add(_listener);

            if (!string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = appInsightsInstrumentationKey;
                _telemetry = new TelemetryClient();
            }
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(_listener);
        }

        public void TestFinished(TestResult result)
        {
            if (result.IsSuccess)
            {
                Trace.Write("Passed");
                Trace.Write(Environment.NewLine);

                if (_telemetry != null)
                {
                    if (result.Name.IndexOf("e2e", StringComparison.InvariantCultureIgnoreCase) > -1
                        || result.Name.IndexOf("endtoend", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        _telemetry.TrackMetric("Performance." + result.Name, result.Time);
                    }
                }
            }

            if (result.IsError)
            {
                Trace.TraceWarning("***Failed***");
                Trace.Write(Environment.NewLine);
                if (result.Message != null) { Debug.WriteLine(result.Message); }
                if (result.StackTrace != null) { Debug.WriteLine(result.StackTrace); };
            }

            if (result.IsFailure)
            {
                Trace.TraceWarning("***Failed***");
                Trace.Write(Environment.NewLine);
                if (result.Message != null) { Debug.WriteLine(result.Message); }
                if (result.StackTrace != null) { Debug.WriteLine(result.StackTrace); };
            }

            if (result.ResultState == ResultState.Ignored)
            {
                Trace.Write("Ignored");
                Trace.Write(Environment.NewLine);
            }
        }

        public void TestStarted(TestName testName)
        {
            Trace.Write((testName.Name + "... ").PadRight(100));
        }

        public void TestOutput(TestOutput testOutput)
        {
        }

        public void UnhandledException(Exception exception)
        {
        }

        public void RunFinished(Exception exception)
        {
        }

        public void RunFinished(TestResult result)
        {
        }

        public void RunStarted(string name, int testCount)
        {
                Trace.Write(Environment.NewLine);
        }

        public void SuiteFinished(TestResult result)
        {
        }

        public void SuiteStarted(TestName testName)
        {
        }
    }
}
