using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using NUnit.Core;
using HealthMonitor.Configuration;
using System.Configuration;
using System.Diagnostics;

namespace HealthMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sendEmailReport = false;
            var appName = "Unspecified App";
            var ensureTerminalsStartup = false;
            var selfHosting = false;
            var connectionString = string.Empty;
            var specificTest = string.Empty;
            int errorCount = 0;

            Debug.AutoFlush = true;

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
                    else if (i > 0 && args[i - 1] == "--connectionString" && args[i] != null)
                    {
                        connectionString = args[i];
                    }
                    else if (args[i] == "--self-hosting")
                    {
                        selfHosting = true;
                    }
                    else if (i > 0 && args[i - 1] == "--test" && args[i] != null)
                    {
                        specificTest = args[i];
                    }
                }

                if (selfHosting)
                {
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new ArgumentException("You should specify --connectionString \"{Name}={Value}\" argument when using self host mode.");
                    }

                    var regex = new System.Text.RegularExpressions.Regex("([\\w\\d]{1,})=([\\s\\S]+)");
                    var match = regex.Match(connectionString);
                    if (match == null || !match.Success || match.Groups.Count != 3)
                    {
                        throw new ArgumentException("Please specify connection string in the following format: \"{Name}={Value}\".");
                    }
                    UpdateConnectionString(match.Groups[1].Value, match.Groups[2].Value);
                }
            }

            var selfHostInitializer = new SelfHostInitializer();
            if (selfHosting)
            {
                selfHostInitializer.Initialize(connectionString);
            }

            try
            {
                errorCount = new Program().Run(ensureTerminalsStartup, sendEmailReport, appName, specificTest);
            }
            catch (Exception)
            {

            }
            finally
            {
                if (selfHosting)
                {
                    selfHostInitializer.Dispose();
                }
            }
            Environment.Exit(errorCount);
        }

        private static void UpdateConnectionString(string key, string value)
        {
            System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.ConnectionStrings.ConnectionStrings[key].ConnectionString = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("connectionStrings");
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

        private int Run(
            bool ensureTerminalsStartup,
            bool sendEmailReport,
            string appName,
            string test)
        {
            CoreExtensions.Host.InitializeService();

            if (ensureTerminalsStartup)
            {
                EnsureTerminalsStartUp();
            }

            var testRunner = new NUnitTestRunner();
            var report = testRunner.Run(test);

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
            return report.Tests.Count(x => !x.Success);
        }
    }
}
