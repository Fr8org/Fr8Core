using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using KwasantCore.ExternalServices;

namespace Daemons
{
    public class DiagnosticsTest : Daemon<DiagnosticsTest>
    {
        private readonly Dictionary<String, MethodInfo> _methodMap;
        private readonly object _controller;
        private const string runAllKey = "DiagnosticsTest_RunAll";
        private readonly object _testRunLocker = new object();

        protected override String ServiceGroupName { get { return "Tests (Daemon)"; } }

        public DiagnosticsTest()
        {
            lock (_testRunLocker)
            {
                //Super hacky, but it gets the job done.
                //We can't reference KwasantWeb, as Daemons are being referenced by it, which causes a circular dependency.
                //This will find the controller based on run-time reflection
                var kwasantWebAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == "KwasantWeb, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (kwasantWebAssembly == null)
                    throw new Exception("Could not find KwasantWeb assembly.");
                
                var diagControllerType = kwasantWebAssembly.GetType("KwasantWeb.Controllers.DiagnosticsController");
                if (diagControllerType == null)
                    throw new Exception("Could not find DiagnosticsController type in KwasantWeb assembly.");

                _controller = Activator.CreateInstance(diagControllerType);
                _methodMap = diagControllerType.GetMethods().ToDictionary(m => m.Name, m => m);

                AddTest(runAllKey, "Run all tests");
            }
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromHours(1).TotalMilliseconds; }
        }

        protected override void Run()
        {
            RunAllTests();
        }

        public void RunAllTests()
        {
            LogEvent("Running all tests..");
            lock (_testRunLocker)
            {
                var services = ServiceManager.GetServicesKeys();

                foreach (var serviceKey in services)
                {
                    var service = ServiceManager.GetInformationForService(serviceKey);

                    foreach (var test in service.Tests)
                    {
                        if (test.Key == runAllKey) //Don't run our own test.. that breaks it! 
                            continue;

                        try
                        {
                            var serverCall = test.Key;
                            var method = _methodMap[serverCall];
                            
                            service.RunningTest = true;
                            method.Invoke(_controller, new object[] { test.Key, test.Value });

                            LogEvent("Running test '" + test.Key + "' on service '" + service.ServiceName + "'.");
                            while (service.RunningTest)
                                Thread.Sleep(100);

                            LogSuccess("Successfully dispatched test '" + test.Key + "' on service '" + service.ServiceName + "'.");
                        }
                        catch (Exception ex)
                        {
                            LogFail(ex, "Failed to dispatch test '" + test.Key + "' on service '" + service.ServiceName + "'.");
                            throw;
                        }
                    }
                }
            }
            LogEvent("All tests dispatched.");
        }
    }
}
