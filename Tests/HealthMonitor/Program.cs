using System;
using System.Configuration;
using System.Linq;
using NUnit.Core;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace HealthMonitor
{
    public class Program
    {
        public static readonly Context Context = new Context();

        static void Main(string[] args)
        {
            var appName = "Unspecified App";
            var csName = "Fr8LocalDB";
            var allArguments = new Dictionary<string, object>();
            var connectionString = ConfigurationManager.ConnectionStrings[csName].ConnectionString;
            var sendEmailReport = false;
            var ensureTerminalsStartup = false;
            var selfHosting = false;
            var specificTest = string.Empty;
            var appInsightsInstrumentationKey = string.Empty;
            int errorCount = 0;
            var skipLocal = false;
            var interactive = false;
            var killIISExpress = false;
            var doCleanUp = false;
            var smtpUsername = string.Empty;
            var smtpPassword = string.Empty;

            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    if (i > 0 && args[i - 1].StartsWith("--"))
                    {
                        if (!args[i].StartsWith("--"))
                        {
                            allArguments[args[i - 1].Substring(2)] = args[i];
                        }
                        else
                        {
                            allArguments[args[i - 1].Substring(2)] = true;
                        }
                    }

                    if (args[i] == "--email-report")
                    {
                        sendEmailReport = true;
                    }
                    else if (args[i] == "--ensure-startup")
                    {
                        Console.WriteLine("--ensure-startup argument is no more supported and will be ignored.");
                    }
                    else if (i > 0 && args[i - 1] == "--app-name" && args[i] != null)
                    {
                        appName = args[i];
                    }
                    else if (args[i] == "--self-hosting")
                    {
                        selfHosting = true;
                    }
                    else if (args[i] == "--skip-local")
                    {
                        skipLocal = true;
                    }
                    else if (i > 0 && args[i - 1] == "--test" && args[i] != null)
                    {
                        specificTest = args[i];
                    }

                    // Application Insights instrumentation key. When specified, 
                    // test performance information will be posted to AI for website performance report. 
                    else if (i > 0 && args[i - 1] == "--aiik" && args[i] != null)
                    {
                        appInsightsInstrumentationKey = args[i];
                        Context.InstrumentationKey = appInsightsInstrumentationKey;
                    }

                    //This flag will signal HM to wait for user response before shut down (used when launched directly)
                    else if (args[i] == "--interactive")
                    {
                        interactive = true;
                    }
                    else if (args[i] == "--killexpress")
                    {
                        killIISExpress = true;
                    }

                    // Execute the clean-up script after the tests finished running. 
                    else if (args[i].ToLowerInvariant() == "--cleanup")
                    {
                        doCleanUp = true;
                    }
                    else if (args[i] == "--smtp-user")
                    {
                        smtpUsername = args[i];
                    }
                    else if (args[i] == "--smtp-pass")
                    {
                        smtpPassword = args[i];
                    }
                }

                if (killIISExpress)
                {
                    try
                    {
                        foreach (var iisExpressProcess in Process.GetProcessesByName("iisexpress"))
                        {
                            iisExpressProcess.Kill();
                        }
                    }
                    catch
                    {
                        //Do nothing. This may mean that user have no access to killing processes
                    }
                }
            }

            Context.ConnectionString = connectionString;
            Context.AllArguments = allArguments;

            var selfHostInitializer = new SelfHostInitializer();
            if (selfHosting)
            {
                selfHostInitializer.Initialize(csName + "=" + connectionString);
            }

            try
            {
                errorCount = new Program().Run(
                    ensureTerminalsStartup,
                    sendEmailReport,
                    appName,
                    specificTest,
                    skipLocal,
                    appInsightsInstrumentationKey,
                    smtpUsername,
                    smtpPassword
                );
            }
            finally
            {
                if (selfHosting)
                {
                    selfHostInitializer.Dispose();
                }
            }

            if (doCleanUp)
            {
                Trace.TraceWarning("Running clean-up scripts...");
                new CleanupService().LaunchCleanup(connectionString);
            }

            if (errorCount > 0)
            {
                Trace.TraceWarning($"{errorCount} tests failed");
            }

            if (interactive)
            {
                Console.WriteLine("Tests are completed. Press ENTER to close the app");
                Console.ReadLine();
            }

            Environment.Exit(errorCount);
        }

        private int Run(
            bool ensureTerminalsStartup,
            bool sendEmailReport,
            string appName,
            string test,
            bool skipLocal,
            string appInsightsInstrumentationKey,
            string smtpUsername,
            string smtpPassword)
        {
            CoreExtensions.Host.InitializeService();

            var testRunner = new NUnitTestRunner(appInsightsInstrumentationKey);
            var report = testRunner.Run(test, skipLocal);

            var failedTestsCount = report.Tests.Count(x => !x.Success);

            if (failedTestsCount > 0)
            {
                var failedTests = report.Tests.Where(x => !x.Success);
                ShowFailedTests(failedTests);

                if (failedTestsCount < 3)
                {
                    Trace.TraceWarning("Failed tests number is " + failedTestsCount + ". This can be caused by some transient errors during build.");
                    Trace.TraceWarning("Running those failed tests again...");

                    foreach (var failedTest in failedTests)
                    {
                        StringBuilder sb = new StringBuilder(failedTest.Name);
                        var indexOfLastDot = failedTest.Name.LastIndexOf('.');
                        sb[indexOfLastDot] = '#';

                        var failedTestName = sb.ToString();
                        var reportAfterRetry = testRunner.Run(failedTestName, skipLocal);

                        if (reportAfterRetry.Tests.First().Success)
                        {
                            var updatedReport = report.Tests.Where(x => x.Name != failedTest.Name).ToList();
                            updatedReport.AddRange(reportAfterRetry.Tests);
                            report.Tests = updatedReport;
                        }
                    }
                }
            }

            if (sendEmailReport)
            {
                if (report.Tests.Any(x => !x.Success))
                {
                    var reportBuilder = new HtmlReportBuilder();
                    var htmlReport = reportBuilder.BuildReport(appName, report);

                    var reportNotifier = new TestReportNotifier();
                    reportNotifier.Notify(appName, htmlReport, smtpUsername, smtpPassword);
                }
            }
            return report.Tests.Count(x => !x.Success);
        }

        private void ShowFailedTests(IEnumerable<TestReportItem> failedTests)
        {
            Trace.TraceWarning("Failed tests: ");
            Trace.Indent();
            foreach (var failedTest in failedTests)
            {
                Trace.TraceWarning(failedTest.Name);
            }
            Trace.Unindent();
        }
    }
}

