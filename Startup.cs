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
using Hub.Managers;
using Utilities;
using Utilities.Configuration.Azure;
using Hub.Interfaces;
using Hangfire;
using Hangfire.StructureMap;
using Hangfire.Dashboard;

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
            ConfigureAuth(app);


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
                AuthorizationFilters = new[] { new MyRestrictiveAuthorizationFilter() }
            });
            app.UseHangfireServer();
        }

        public class MyRestrictiveAuthorizationFilter : IAuthorizationFilter
        {
            public bool Authorize(IDictionary<string, object> owinEnvironment)
            {
                var context = new OwinContext(owinEnvironment);
                if (context.Authentication.User.Identity.Name == "hangfireuser@fr8.co")
                    return true;
                else return false;
            }
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

            // At Startup Check If the Log Monitor Fr8 Event plan exist in the database then active it. otherwise create the new plan.

            PlanManager manager = new PlanManager();
            string sytemUserEmail = ObjectFactory.GetInstance<IConfigRepository>().Get<string>("SystemUserEmail");
            await manager.CreatePlan_LogFr8InternalEvents(sytemUserEmail).ConfigureAwait(true);
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }
    }
}
