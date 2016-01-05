using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Segment;
using StructureMap;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Hub.ExternalServices;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using HubWeb.ViewModels;
using Utilities;
using Logger = Utilities.Logging.Logger;

namespace HubWeb.Controllers
{
	[ DockyardAuthorize( Roles = "Booker" ) ]
    public class DiagnosticsController : Controller
    {
		private readonly ITime _time;

		public DiagnosticsController()
		{
			_time = ObjectFactory.GetInstance<ITime>();
		}

        public ActionResult Index(int? pageAmount)
        {
            if (!pageAmount.HasValue)
                pageAmount = 100;
            var serviceTypes = ServiceManager.GetServicesKeys();

            var vm = serviceTypes.Select(st =>
            {
                var info = ServiceManager.GetInformationForService(st);
                var percent = info.Attempts == 0 ? 0 : (int) Math.Round(100.0*info.Success/info.Attempts);
                var lastUpdated = info.Events.Any() ? info.Events.Max(e => e.Item1).TimeAgo() : "Never";
                var lastSuccess = info.LastSuccess.HasValue ? info.LastSuccess.Value.TimeAgo() : "Never";
                var lastFail = info.LastFail.HasValue ? info.LastFail.Value.TimeAgo() : "Never";

                bool operational;
                if (!info.LastSuccess.HasValue)
                {
                    operational = !info.LastFail.HasValue;
                }
                else
                {
                    if (info.LastFail.HasValue)
                        operational = info.LastSuccess > info.LastFail;
                    else
                        operational = true;
                }

                return new DiagnosticInfoVM
                {
                    Attempts = info.Attempts,
                    Success = info.Success,
                    Percent = percent,
                    ServiceName = info.ServiceName,
                    LastUpdated = lastUpdated,
                    GroupName = info.GroupName,
                    LastFail = lastFail,
                    RunningTest = info.RunningTest,
                    LastSuccess = lastSuccess,
                    Operational = operational,
                    Flags = info.Flags,
                    Tests =
                        info.Tests.Select(a => new DiagnosticActionVM {ServerAction = a.Key, DisplayName = a.Value})
                            .ToList(),
                    Actions =
                        info.Actions.Select(a => new DiagnosticActionVM {ServerAction = a.Key, DisplayName = a.Value})
                            .ToList(),
                    Key = info.Key,
                    Events =
                        info.Events.AsEnumerable()
                            .Reverse()
                            .Where(e => !String.IsNullOrEmpty(e.Item2))
                            .Take(pageAmount.Value)
                            .Select(
                                e =>
                                    new DiagnosticEventInfoVM
                                    {
                                        Date = e.Item1.ToString(),
                                        EventName = e.Item2.Replace(Environment.NewLine, "<br/>")
                                    })
                            .ToList()
                };
            }).ToList();
            return View(vm);
        }


        [HttpPost]
        public ActionResult StartDaemon(String key)
        {
            var daemon = ServiceManager.GetInformationForService(key).Instance as Daemon;
            if (daemon != null)
            {
                daemon.Start();
                return Json(true);
            }
            return Json(false);
        }

        [HttpPost]
        public ActionResult StopDaemon(String key)
        {
            var daemon = ServiceManager.GetInformationForService(key).Instance as Daemon;
            if (daemon != null)
            {
                daemon.Stop();
                return Json(true);
            }
            return Json(false);
        }

        [HttpPost]
        public ActionResult DiagnosticsTest_RunAll(String key, String testName)
        {
            var diagnosticsTest = ServiceManager.GetInformationForService<DiagnosticsTest>().Instance as DiagnosticsTest;
            if (diagnosticsTest == null)
                return Json(false);

            return RunAsync(diagnosticsTest.RunAllTests);
        }

        [HttpPost]
        public ActionResult OutboundEmailDaemon_Test(String key, String testName)
        {
            //return RunAsync(() => SendTestEmail(testName, (uow, curEmail) => uow.EnvelopeRepository.ConfigurePlainEmail(curEmail)));
            return null;
        }

        private JsonResult RunAsync(ThreadStart action)
        {
            try
            {
                new Thread(action).Start();
                return Json(true);
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Failed to run test", ex);
                return Json(false);
            }
        }

        private static void MarkRunningTest<T>(String testName)
        {
            ServiceManager.StartingTest<T>();
            ServiceManager.LogEvent<T>("Running test '" + testName + "'...");
        }

        private static void MarkTestSuccess<T>(String testName)
        {
            ServiceManager.FinishedTest<T>();
            ServiceManager.LogEvent<T>("Test '" + testName + "' succeeded.");
            ServiceManager.LogSuccess<T>();
        }

        private static void MarkTestFail<T>(String testName, string results, DateTime currentTime)
        {
            ServiceManager.FinishedTest<T>();
            ServiceManager.LogEvent<T>("Test '" + testName + "' failed.");
            ServiceManager.LogFail<T>();

            if (!Utilities.Server.IsDevMode)
            {

                //Dispatch an email which alerts us about failed tests
                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                    string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                    Email email = ObjectFactory.GetInstance<Email>();
					
                    string message = String.Format(@"
Test failed at {0}. Results:
{1}.
See more: {2}
", currentTime, results, Utilities.Server.ServerUrl);
                    string subject = String.Format("Alert! Service test failed. Service: {0} Test: {1}", typeof (T).Name,
                        testName);
                    var curEmail = email.GenerateBasicMessage(uow, subject, message, fromAddress, "ops@kwasant.com");
                    //uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                    uow.SaveChanges();
                }
            }
        }

        public void SendTestEmail(String testName, Action<IUnitOfWork, EmailDO> configureEmail)
        {
            MarkRunningTest<OutboundEmail>(testName);
            MarkRunningTest<InboundEmail>(testName);

            var inboundEmailDaemon = ServiceManager.GetInformationForService<InboundEmail>().Instance as InboundEmail;
            if (inboundEmailDaemon == null)
            {
				MarkTestFail<OutboundEmail>(testName, "No InboundEmail daemon found.", _time.CurrentDateTime());
				MarkTestFail<InboundEmail>(testName, "No InboundEmail daemon found.", _time.CurrentDateTime());
                return;
            }

            var subjKey = Guid.NewGuid().ToString();

            inboundEmailDaemon.RegisterTestEmailSubject(subjKey);
            bool messageReceived = false;

            EventHandler<TestMessageReceivedEventArgs> testMessageReceived = (sender, args) =>
            {
                if (subjKey == args.Subject)
                {
                    messageReceived = true;
                    args.DeleteFromInbox = true;
                }
            };

            InboundEmail.TestMessageReceived += testMessageReceived;
            EmailDO createdEmailDO;
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Email email = ObjectFactory.GetInstance<Email>();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                const string message = "This is a test message";
                string subject = subjKey;
                createdEmailDO = email.GenerateBasicMessage(uow, subject, message, fromAddress,
                    inboundEmailDaemon.GetUserName());
                configureEmail(uow, createdEmailDO);
                uow.SaveChanges();

                ServiceManager.LogEvent<OutboundEmail>("Queued email to " + inboundEmailDaemon.GetUserName());
            }

            bool success = false;
	        DateTime currentTime = _time.CurrentDateTime();
			var startTime = currentTime;
            var endTime = startTime.Add(TimeSpan.FromMinutes(10));
			while (!success && _time.CurrentDateTime() < endTime)
            {
                if (messageReceived)
                {
                    MarkTestSuccess<OutboundEmail>(testName);
                    MarkTestSuccess<InboundEmail>(testName);
                    success = true;
                }

                Thread.Sleep(100);
            }

            if (!success)
            {
                const string errorMessage = "No email was reported with the correct subject within the given timeframe.";
				MarkTestFail<OutboundEmail>(testName, errorMessage, _time.CurrentDateTime());
				MarkTestFail<InboundEmail>(testName, errorMessage, _time.CurrentDateTime());
            }

            //Now, delete that email
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var savedEmailDO = uow.EmailRepository.GetByKey(createdEmailDO.Id);
                if (savedEmailDO != null)
                {
                    var recipients = savedEmailDO.Recipients.ToList();
                    foreach (var recipient in recipients)
                        uow.RecipientRepository.Remove(recipient);
                    //var envelopes = uow.EnvelopeRepository.GetQuery().Where(env => env.EmailID == savedEmailDO.Id).ToList();
                    //foreach(var envelope in envelopes)
                    //    uow.EnvelopeRepository.Remove(envelope);
                    //uow.EmailRepository.Remove(savedEmailDO);
                }

                uow.SaveChanges();
            }
        }
    }
}