using System;
using System.Configuration;
using System.Linq;
using NUnit.Core;

namespace HealthMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sendEmailReport = false;
            var appName = "Unspecified App";
            var ensureTerminalsStartup = false;

            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    if (args[i] == "--email-report")
                    {
                        sendEmailReport = true;
                    }
                    else if (args[i] == "--ensure-startup")
                    {
                        ensureTerminalsStartup = true;
                    }
                    else if (i > 0 && args[i - 1] == "--app-name" && args[i] != null)
                    {
                        appName = args[i];
                    }
                }
            }

            new Program().Run(ensureTerminalsStartup, sendEmailReport, appName);
        }

        private void EnsureTerminalsStartUp()
        {
            var awaiter = new TerminalStartUpAwaiter();
            var failedToStart = awaiter.AwaitStartUp();

            if (failedToStart.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Following terminals have failed to start:");

                foreach (var terminalName in failedToStart)
                {
                    Console.WriteLine(terminalName);
                }

                Environment.Exit(failedToStart.Count);
            }
        }

        private void ReportToConsole(string appName, TestReport report)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Application: {0}", appName);
            Console.WriteLine("Integration tests result: {0} / {1} passed", report.Tests.Count(x => x.Success), report.Tests.Count());
            Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var test in report.Tests.Where(x => !x.Success))
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Integration Test Failure: {0}", test.Name);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Message: {0}", test.Message);
                Console.WriteLine("StackTrace: {0}", test.StackTrace);
            }
        }

        private void Run(bool ensureTerminalsStartup, bool sendEmailReport, string appName)
        {
            CoreExtensions.Host.InitializeService();

            if (ensureTerminalsStartup)
            {
                EnsureTerminalsStartUp();
            }

            var testRunner = new NUnitTestRunner();
            var report = testRunner.Run();

            if (sendEmailReport)
            {
                if (report.Tests.Any(x => !x.Success))
                {
                    var reportBuilder = new HtmlReportBuilder();
                    var htmlReport = reportBuilder.BuildReport(appName, report);

                    var reportNotifier = new TestReportNotifier();
                    reportNotifier.Notify(appName, htmlReport);
                }
            }

            ReportToConsole(appName, report);

            var errorCount = report.Tests.Count(x => !x.Success);
            Environment.Exit(errorCount);
        }
    }
}
