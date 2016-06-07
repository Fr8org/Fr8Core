using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using fr8.Infrastructure.Utilities;
using fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Security;
using Hangfire;
using Hangfire.StructureMap;

[assembly: OwinStartup(typeof(HubWeb.Startup))]

namespace HubWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public async void Configuration(IAppBuilder app, bool selfHostMode)
        {
            //ConfigureDaemons();
            // ConfigureAuth(app);
            OwinInitializer.ConfigureAuth(app, "/DockyardAccount/Index");


            ConfigureHangfire(app, "DockyardDB");

            if (!selfHostMode)
            {
                await RegisterTerminalActions();
            }
        }

        public void ConfigureHangfire(IAppBuilder app, string connectionString)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString)
                .UseStructureMapActivator(ObjectFactory.Container);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] { new HangFireAuthorizationFilter() },
            });
            app.UseHangfireServer();
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
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

            var terminalUrls = FileUtils.LoadFileHostList();
            int count = 0;
            var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            var terminalService = ObjectFactory.GetInstance<ITerminal>();


            foreach (string url in terminalUrls)
            {
                try
                {
                    var activityTemplateList = (await terminalService.GetAvailableActivities(url)).ToList();

                    foreach (var curItem in activityTemplateList)
                    {
                        try
                        {
                            activityTemplate.RegisterOrUpdate(curItem);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            alertReporter.ActivityTemplateTerminalRegistrationError(
                                $"Failed to register {curItem.Terminal.Name} terminal. Error Message: {ex.Message}",
                                ex.GetType().Name);
                        }
                    }

                    activityTemplate.RemoveInactiveActivities(activityTemplateList);
                }
                catch (Exception ex)
                {
                    alertReporter.ActivityTemplateTerminalRegistrationError(
                        string.Format("Failed terminal service: {0}. Error Message: {1} ", url, ex.Message),
                        ex.GetType().Name);
                }
            }

            alertReporter.ActivityTemplatesSuccessfullyRegistered(count);
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }
    }
}
