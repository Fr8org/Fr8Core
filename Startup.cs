using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Hub.Managers;
using Hub.Services;
using Utilities;
using Utilities.Configuration.Azure;
using Utilities.Logging;
using Hub.Interfaces;
using Hangfire;
using Hub.Managers.APIManagers.Transmitters.Restful;

[assembly: OwinStartup(typeof(HubWeb.Startup))]

namespace HubWeb
{
    public partial class Startup
    {
        private class TerminalIdSecretMatch
        {
            public TerminalIdSecretMatch(string terminalName, string terminalVersion, string terminalId, string terminalSecret)
            {
                this.TerminalId = terminalId;
                this.TerminalSecret = terminalSecret;
                this.TerminalName = terminalName;
                this.TerminalVersion = terminalVersion;
            }

            public string TerminalName { get; set; }
            public string TerminalVersion { get; set; }
            public string TerminalId { get; set; }
            public string TerminalSecret { get; set; }
        }

        private readonly List<TerminalIdSecretMatch> _staticTerminalSecrets = new List<TerminalIdSecretMatch>()
            {
                { new TerminalIdSecretMatch("terminalSalesforce", "1","d814af88-72b3-444c-9198-8c62292f0be5","3b685a89-314d-48ce-91c6-7b1cfa29aa21")},
                { new TerminalIdSecretMatch("terminalAzure", "1","e134e36f-9f63-4109-b913-03498d9356b1","8d3d33d9-a260-46e2-b25a-121f2aba2a54")},
                { new TerminalIdSecretMatch("terminalFr8Core", "1","2db48191-cda3-4922-9cc2-a636e828063f","9b4a97f3-97ea-42d7-8b02-a208ea47d760")},
                { new TerminalIdSecretMatch("terminalDocuSign", "1","ee29c5bc-b9e7-49c5-90e1-b462c7e320e9","cc426e06-a42a-4193-9b90-d1122be979a3")},
                { new TerminalIdSecretMatch("terminalSlack", "1","8783174f-7fb7-4947-98af-4f1cdd8b394f","aa43d09e-a0dd-4433-8b05-4485e57738c6")},
                { new TerminalIdSecretMatch("terminalTwilio", "1","2dd73dda-411d-4e18-8e0a-54bbe1aa015b","3a772e7d-1368-4173-b081-91a7318910c7")},
                { new TerminalIdSecretMatch("terminalExcel", "1","551acd9b-d91d-4de7-a0ba-8c61be413635","36392f9d-c3c0-4b6a-a54a-142ba1ce312f")},
                { new TerminalIdSecretMatch("terminalSendGrid", "1","7eab0e8    1-288c-492b-88e5-c49e9aae38da","a3a65c3c-6d75-4fd6-bd76-e66047affe09")},
                { new TerminalIdSecretMatch("terminalGoogle", "1","1a170d44-841f-4fa2-aae4-b17ad6c469ec","ee7a622b-4a12-4dd6-ac09-03caf0da0f25")},
                { new TerminalIdSecretMatch("terminalDropbox", "1","c471e51e-1b2d-4751-b155-4af03ef51c3a","f6e4a687-fc0b-462a-87de-9cb2729d2bc1")},
                { new TerminalIdSecretMatch("terminalPapertrail", "1","9b21279b-efb4-493a-a02b-fe8694262cc8","42783cd2-d5e1-4d5a-9ea8-b63922ce2e20")},
                { new TerminalIdSecretMatch("terminalQuickBooks", "1","75ec4967-6113-43b5-bb4c-6b3468696e57","749f5c59-1bf1-4cb6-9275-eb1d489d9a05")},
                { new TerminalIdSecretMatch("terminalYammer", "1","f2b999be-be3f-42b5-b0d5-611d0606723b","d14aaa44-22a1-4d2c-b14b-be559c8941b5")},
                { new TerminalIdSecretMatch("terminalAtlassian", "1","d770ec3c-975b-4ca8-910e-a55ac43af383","f747e49c-63a8-4a1b-8347-dd2e436c3b36")}
                
            };

        public async void Configuration(IAppBuilder app)
        {
            //ConfigureDaemons();
            ConfigureAuth(app);

#if RELEASE
            ConfigureHangfire(app, "DockyardDB");
#endif 
            await RegisterTerminalActions();
        }

        public void ConfigureHangfire(IAppBuilder app, string connectionString)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);

#if RELEASE
            app.UseHangfireDashboard();
            app.UseHangfireServer();
#endif
        }

        //SeedDatabases
        //Ensure that critical configuration information is present in the database
        //Twilio SMS messages are based on CommunicationConfigurations
        //Database should have a CommunicationConfiguration that sends an SMS to 14158067915
        //Load Repository and query for CommunicationConfigurations. If null, create one set to 14158067915
        //If not null, make sure that at least one exists where the ToAddress is 14158067915
        public void ConfigureCommunicationConfigs()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = uow.CommunicationConfigurationRepository;
            List<CommunicationConfigurationDO> curConfigureCommunicationConfigs = communicationConfigurationRepo.GetAll().ToList();



            if (curConfigureCommunicationConfigs.Find(config => config.ToAddress == CloudConfigurationManager.GetSetting("MainSMSAlertNumber")) == null)
            // it is not true that there is at least one commConfig that has the Main alert number
            {
                CommunicationConfigurationDO curCommConfig = new CommunicationConfigurationDO();
                curCommConfig.CommunicationType = CommunicationType.Sms;
                curCommConfig.ToAddress = CloudConfigurationManager.GetSetting("MainSMSAlertNumber");
                communicationConfigurationRepo.Add(curCommConfig);
                uow.SaveChanges();
            }

        }

        public void AddMainSMSAlertToDb(CommunicationConfigurationRepository communicationConfigurationRepo)
        {

        }

        // @alexavrutin here: Daemon-related code needs to be reworked, the code below is no more actual. 

        //private static void ConfigureDaemons()
        //{
        //    DaemonSettings daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
        //    if (daemonConfig != null)
        //    {
        //        if (daemonConfig.Enabled)
        //        {
        //            foreach (DaemonConfig daemon in daemonConfig.Daemons)
        //            {
        //                try
        //                {
        //                    if (daemon.Enabled)
        //                    {
        //                        Type type = Type.GetType(daemon.InitClass, true);
        //                        Daemon obj = Activator.CreateInstance(type) as Daemon;
        //                        if (obj == null)
        //                            throw new ArgumentException(
        //                                string.Format(
        //                                    "A daemon must implement IDaemon. Type '{0}' does not implement the interface.",
        //                                    type.Name));
        //                        obj.Start();
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    Logger.GetLogger().Error("Error initializing daemon '" + daemon.Name + "'.", e);
        //                }
        //            }
        //        }
        //    }
        //}

        public async Task RegisterTerminalActions()
        {
            var alertReporter = ObjectFactory.GetInstance<EventReporter>();

            var activityTemplateHosts = Utilities.FileUtils.LoadFileHostList();
            int count = 0;
            var uri = string.Empty;

            var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            var terminalService = ObjectFactory.GetInstance<ITerminal>();

            foreach (string url in activityTemplateHosts)
            {
                try
                {
                    uri = url.StartsWith("http") ? url : "http://" + url;
                    uri += "/terminals/discover";

                    var activityTemplateList = await terminalService.GetAvailableActions(uri);
                    
                    foreach (var curItem in activityTemplateList)
                    {
                        try
                        {
                            terminalService.RegisterOrUpdate(curItem.Terminal);
                            activityTemplate.RegisterOrUpdate(curItem);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            alertReporter = ObjectFactory.GetInstance<EventReporter>();
                            alertReporter.ActivityTemplateTerminalRegistrationError(
                                string.Format("Failed to register {0} terminal. Error Message: {1}", curItem.Terminal.Name, ex.Message),
                                ex.GetType().Name);
                        }

                    }
                }
                catch (Exception ex)
                {
                    alertReporter = ObjectFactory.GetInstance<EventReporter>();
                    alertReporter.ActivityTemplateTerminalRegistrationError(
                        string.Format("Failed terminal service: {0}. Error Message: {1} ", uri, ex.Message),
                        ex.GetType().Name);

                }
            }

            alertReporter.ActivityTemplatesSuccessfullyRegistered(count);
            SetStaticTerminalSecrets();
        }

        private void SetStaticTerminalSecrets()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var termSecret in _staticTerminalSecrets)
                {
                    var terminal = uow.TerminalRepository.GetQuery().FirstOrDefault(t => t.Name == termSecret.TerminalName && t.Version == termSecret.TerminalVersion);
                    if (terminal != null)
                    {
                        terminal.PublicIdentifier = termSecret.TerminalId;
                        terminal.Secret = termSecret.TerminalSecret;
                    }
                }

                uow.SaveChanges();
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }
    }
}
