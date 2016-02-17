using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Core;
using System.IO;
using System.Diagnostics;

namespace HealthMonitor
{
    public class NUnitTraceListener : EventListener, IDisposable
    {
        ConsoleTraceListener _listener = new ConsoleTraceListener();

        public NUnitTraceListener()
        {
            //Remove OWIN listeners to avoid multiple copies of message getting to console
            var listeners = Trace.Listeners.OfType<TextWriterTraceListener>().Where(l => l.Name == "HostingTraceListener").ToList();
            foreach (var l in listeners)
            {
                Trace.Listeners.Remove("HostingTraceListener");
            }

            Trace.Listeners.Add(_listener);
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
            }

            if (result.IsError)
            {
                Trace.Write("Failed");
                Trace.Write(Environment.NewLine);
                if (result.Message != null) { Debug.WriteLine(result.Message); }
                if (result.StackTrace != null) { Debug.WriteLine(result.StackTrace); };
            }

            if (result.IsFailure)
            {
                Trace.Write("Failed");
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
            Trace.Write(String.Format("{0}...    ", testName.Name));
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
        }

        public void SuiteFinished(TestResult result)
        {
        }

        public void SuiteStarted(TestName testName)
        {
        }
    }
}
