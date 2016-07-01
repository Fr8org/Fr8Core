using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Security;
using Hangfire;
using Hangfire.StructureMap;
using Hub.ModelBinders;
using Hub.StructureMap;
using HubWeb.App_Start;
using Segment;

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
            ObjectFactory.Configure(Fr8.Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            ObjectFactory.GetInstance<AutoMapperBootStrapper>().ConfigureAutoMapper();

            var db = ObjectFactory.GetInstance<DbContext>();
            db.Database.Initialize(true);
            
            EventReporter curReporter = ObjectFactory.GetInstance<EventReporter>();
            curReporter.SubscribeToAlerts();

            IncidentReporter incidentReporter = ObjectFactory.GetInstance<IncidentReporter>();
            incidentReporter.SubscribeToAlerts();
            
            StartupMigration.CreateSystemUser();
            StartupMigration.MoveSalesforceRefreshTokensIntoKeyVault();

            SetServerUrl();

            OwinInitializer.ConfigureAuth(app, "/DockyardAccount/Index");
            
            ConfigureHangfire(app, "DockyardDB");

#pragma warning disable 4014
            RegisterTerminalActions(selfHostMode);
#pragma warning restore 4014
        }

        private void SetServerUrl()
        {
            var config = ObjectFactory.GetInstance<IConfigRepository>();

            var serverProtocol = config.Get("ServerProtocol", String.Empty);
            var domainName = config.Get("ServerDomainName", String.Empty);
            var domainPort = config.Get<int?>("ServerPort", null);

            if (!String.IsNullOrWhiteSpace(domainName) && !String.IsNullOrWhiteSpace(serverProtocol) && domainPort.HasValue)
            {
                Server.ServerUrl = $"{serverProtocol}{domainName}{(domainPort.Value == 80 ? String.Empty : (":" + domainPort.Value))}/";
                Server.ServerHostName = domainName;
            }
        }

        private async Task RegisterTerminalActions(bool selfHostMode)
        {
            var terminalDiscovery = ObjectFactory.GetInstance<ITerminalDiscoveryService>();

            await terminalDiscovery.Discover();

            if (!selfHostMode)
            {
#pragma warning disable 4014 
                //We don't await this call as this is Hangfire dispatcher job
                ObjectFactory.GetInstance<IJobDispatcher>().Enqueue(() => StartMonitoringManifestRegistrySubmissions());
#pragma warning restore 4014
            }
        }

        public static async Task StartMonitoringManifestRegistrySubmissions()
        {
            await ObjectFactory.GetInstance<IManifestRegistryMonitor>().StartMonitoringManifestRegistrySubmissions();
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

       

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }
    }
}
