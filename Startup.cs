using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Security;
using Hangfire;
using Hangfire.StructureMap;
using Hub.StructureMap;
using HubWeb.App_Start;
using GlobalConfiguration = Hangfire.GlobalConfiguration;
using System.Globalization;
using System.Threading;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Enums;
using Hub.Services;
using HubWeb.Infrastructure_PD;

[assembly: OwinStartup(typeof(HubWeb.Startup))]

namespace HubWeb
{
    public class Startup : IHttpControllerActivator
    {
        public void Configuration(IAppBuilder app)
        {
            //fix to IIS hanging on MemoryCache initialization
            //taken from here https://www.zpqrtbnk.net/posts/appdomains-threads-cultureinfos-and-paracetamol
            var currentCulture = CultureInfo.CurrentCulture;
            var invariantCulture = currentCulture;
            while (invariantCulture.Equals(CultureInfo.InvariantCulture) == false)
                invariantCulture = invariantCulture.Parent;
            if (!(ReferenceEquals(invariantCulture, CultureInfo.InvariantCulture)))
            {
                var thread = Thread.CurrentThread;
                thread.CurrentCulture = CultureInfo.GetCultureInfo(thread.CurrentCulture.Name);
                thread.CurrentUICulture = CultureInfo.GetCultureInfo(thread.CurrentUICulture.Name);
            }

            Configuration(app, false);
        }

        public async void Configuration(IAppBuilder app, bool selfHostMode)
        {
            ObjectFactory.Configure(Fr8.Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);

            //For PlanDirectory merge
            ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);


            ObjectFactory.GetInstance<AutoMapperBootStrapper>().ConfigureAutoMapper();

            var db = ObjectFactory.GetInstance<DbContext>();
            db.Database.Initialize(true);

            EventReporter curReporter = ObjectFactory.GetInstance<EventReporter>();
            curReporter.SubscribeToAlerts();

            IncidentReporter incidentReporter = ObjectFactory.GetInstance<IncidentReporter>();
            incidentReporter.SubscribeToAlerts();

            StartupMigration.UpdateTransitionNames();

            SetServerUrl();

            OwinInitializer.ConfigureAuth(app, "/Account/Index");

            if (!selfHostMode)
            {
                System.Web.Http.GlobalConfiguration.Configure(ConfigureControllerActivator);
            }

            ConfigureHangfire(app, "Fr8LocalDB");

#pragma warning disable 4014
            RegisterTerminalActions(selfHostMode);
#pragma warning restore 4014

            await GenerateManifestPages();

            EnsureMThasaDocuSignRecipientCMTypeStored();
        }

        private void EnsureMThasaDocuSignRecipientCMTypeStored()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWorkFactory>().Create())
            {
                var type = uow.MultiTenantObjectRepository.FindTypeReference(typeof(DocuSignRecipientCM));
                if (type == null)
                {
                    var user = uow.UserRepository.GetQuery().FirstOrDefault();
                    if (user != null)
                    {
                        uow.MultiTenantObjectRepository.Add(new DocuSignRecipientCM() { RecipientId = Guid.NewGuid().ToString() }, user.Id);
                        uow.SaveChanges();
                    }
                }
            }
        }

        private async Task GenerateManifestPages()
        {
            var systemUserAccount = ObjectFactory.GetInstance<Fr8Account>().GetSystemUser();
            if(systemUserAccount == null) return;

            var systemUser = systemUserAccount.EmailAddress?.Address;
            var generator = ObjectFactory.GetInstance<IManifestPageGenerator>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWorkFactory>().Create())
            {
                var generateTasks = new List<Task>();
                foreach (var manifestName in uow.MultiTenantObjectRepository.Query<ManifestDescriptionCM>(systemUser, x => true).Select(x => x.Name).Distinct())
                {
                    generateTasks.Add(generator.Generate(manifestName, GenerateMode.GenerateAlways));
                }
                await Task.WhenAll(generateTasks);
            }
        }

        public void ConfigureControllerActivator(HttpConfiguration configuration)
        {
            configuration.Services.Replace(typeof(IHttpControllerActivator), this);
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

            await terminalDiscovery.DiscoverAll();

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

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return ObjectFactory.GetInstance(controllerType) as IHttpController;
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }
    }
}
