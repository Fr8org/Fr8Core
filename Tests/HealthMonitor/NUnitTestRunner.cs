using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Core;
using Fr8.Testing.Integration;
using System.Reflection;
using System.IO;

namespace HealthMonitor
{
    public class NUnitTestRunner
    {
        readonly string _appInsightsInstrumentationKey;

        public NUnitTestRunner()
        {
        }

        public NUnitTestRunner(string appInsightsInstrumentationKey)
        {
            _appInsightsInstrumentationKey = appInsightsInstrumentationKey;
        }
        
        public class NUnitTestRunnerFilter : ITestFilter
        {
            public bool IsEmpty => false;

            public bool Match(ITest test)
            {
                return test.RunState != RunState.NotRunnable && test.RunState != RunState.Ignored;
            }

            public bool Pass(ITest test)
            {
                return test.RunState != RunState.NotRunnable;
            }
        }

        private Type[] GetTestSuiteTypesUsingReflection(bool skipLocal)
        {
            var integrationTests = new List<Type>();
            var assemblies = GetTestsAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().AsEnumerable<Type>();
                
                var assemblyTestSuits = types.Where(t => t.IsSubclassOf(typeof(BaseIntegrationTest)) 
                    && !t.IsAbstract);
                integrationTests.AddRange(assemblyTestSuits);
            }

            var testSuites = PrepareForRunning(integrationTests);

            if (skipLocal)
            {
                testSuites = testSuites
                    .Where(x => !x.GetCustomAttributes(typeof(SkipLocalAttribute), false).Any());
            }

            return testSuites.ToArray();
        }

        private IEnumerable<Assembly> GetTestsAssemblies()
        {
            var assemblies = new List<Assembly>();            
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            var assembliesDirectoryInfo = new DirectoryInfo(path);
            var assemblyFiles = assembliesDirectoryInfo.GetFiles("terminal*.dll",
                SearchOption.TopDirectoryOnly).Select(f => f.FullName).ToList();
            assemblyFiles.Add(Path.Combine(path, "HubTests.dll"));

            foreach (var assemblyFile in assemblyFiles)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);

                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a =>
                            AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())))
                {
                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                }
                else
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName()));
                    assemblies.Add(assembly);
                }

                //Add HM assembly since here are defined the tasks which are run like tests (e.g. MetricMonitor)
                assemblies.Add(Assembly.GetExecutingAssembly());                
            }
            
            return assemblies;
        }

        private IEnumerable<Type> PrepareForRunning(List<Type> integrationTests)
        {
            // remove duplicates
            var elements = new HashSet<string>();
            integrationTests.RemoveAll(test => !elements.Add(test.FullName));

            // apply proper order
            Predicate<Type> terminalDocusignTests = t => t.FullName.Substring(0, t.FullName.IndexOf('.')).Equals("terminalDocuSignTests");

            var docusignTests = integrationTests.FindAll(terminalDocusignTests);
            Predicate<Type> madseTestsPredicate = t => t.Name.Equals("MonitorAllDocuSignEvents_Tests") || t.Name.Equals("MonitorAllDocuSignEventsLocal_Tests");
            var madseTests = docusignTests.FindAll(madseTestsPredicate);
            docusignTests.RemoveAll(madseTestsPredicate);
            docusignTests.InsertRange(0, madseTests);

            integrationTests.RemoveAll(terminalDocusignTests);
            integrationTests.InsertRange(0, docusignTests);

            var testSuites = integrationTests.AsEnumerable<Type>();
            return testSuites;
        }

        public TestReport Run(string specificTestName = null, bool skipLocal = false)
        {
            var testSuiteTypes = GetTestSuiteTypesUsingReflection(skipLocal);
            if (testSuiteTypes == null || testSuiteTypes.Length == 0)
            {
                return new TestReport()
                {
                    Tests = new List<TestReportItem>()
                };
            }

            var testPackage = new TestPackage("HelthMonitor test package");
            var testSuite = new TestSuite("HelthMonitor test suite");

            TestExecutionContext.CurrentContext.TestPackage = testPackage;

            if (string.IsNullOrEmpty(specificTestName))
            {
                foreach (var testSuiteType in testSuiteTypes)
                {
                    var test = TestFixtureBuilder.BuildFrom(testSuiteType);
                    testSuite.Tests.Add(test);
                }
            }
            else
            {
                Test specificTest = null;
                string specificTestMethod = null;

                if (specificTestName.Contains("#"))
                {
                    var tokens = specificTestName.Split('#');

                    specificTestName = tokens[0];
                    specificTestMethod = tokens[1];
                }

                var specificTestType = testSuiteTypes.FirstOrDefault(x => x.FullName == specificTestName);
                if (specificTestType != null)
                {
                    specificTest = TestFixtureBuilder.BuildFrom(specificTestType);
                }

                if (specificTest != null && !string.IsNullOrEmpty(specificTestMethod))
                {
                    var testsToRemove = new List<Test>();
                    foreach (Test test in specificTest.Tests)
                    {
                        var testMethod = test as TestMethod;
                        if (testMethod != null)
                        {
                            if (testMethod.Method.Name != specificTestMethod)
                            {
                                testsToRemove.Add(test);
                            }
                        }
                    }

                    foreach (var test in testsToRemove)
                    {
                        specificTest.Tests.Remove(test);
                    }
                }

                if (specificTest != null)
                {
                    testSuite.Tests.Add(specificTest);
                }
            }
            
            using (var listener = new NUnitTraceListener(_appInsightsInstrumentationKey))
            {
                var testResult = testSuite.Run(listener, new NUnitTestRunnerFilter());
               
                var testReport = GenerateTestReport(testResult);
                return testReport;
            }
        }

        private TestReport GenerateTestReport(TestResult testResult)
        {
            var reportItems = VisitTestResult(testResult);

            var testReport = new TestReport()
            {
                Tests = reportItems
            };

            return testReport;
        }

        private IEnumerable<TestReportItem> VisitTestResult(TestResult testResult)
        {
            var result = new List<TestReportItem>();

            if (testResult.Test.TestType == "TestMethod")
            {
                var reportItem = new TestReportItem()
                {
                    Name = testResult.FullName,
                    Success = testResult.IsSuccess || (!testResult.IsError && !testResult.IsFailure)
                };

                if (!testResult.IsSuccess)
                {
                    reportItem.Message = testResult.Message;
                    reportItem.StackTrace = testResult.StackTrace;
                }

                result.Add(reportItem);
            }
            else
            {
                if (testResult.Results != null)
                {
                    foreach (TestResult childTestResult in testResult.Results)
                    {
                        result.AddRange(VisitTestResult(childTestResult));
                    }
                }
            }

            return result;
        }
    }
}
