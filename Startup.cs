using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.WindowsAzure;
using Owin;
using StructureMap;

using Configuration;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Utilities;
using Utilities.Logging;
using Utilities.Serializers.Json;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
    public partial class Startup
    {
        public async void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();
            ConfigureAuth(app);

            await RegisterPluginActions();

            LoadLocalActionLists();

            app.Use(async (context, next) =>
            {
                if (string.Equals(context.Request.Method, HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(context.Request.Uri.AbsolutePath, "/api/DocuSignNotification", StringComparison.OrdinalIgnoreCase))
                {
                    var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                    var notificationPortForwardsCsv = configRepository.Get<string>("DocuSignNotificationPortForwards", "");
                    var notificationPortForwards = !string.IsNullOrEmpty(notificationPortForwardsCsv)
                        ? notificationPortForwardsCsv.Split(',')
                        : new string[0];

                    if (notificationPortForwards.Any())
                    {
                        using (var forwarder = new HttpClient())
                        {
                            foreach (var notificationPortForward in notificationPortForwards)
                            {
                                var response = await
                                    forwarder.PostAsync(
                                        new Uri(string.Concat("http://", notificationPortForward, context.Request.Uri.PathAndQuery)),
                                        new StreamContent(context.Request.Body));
                                Logger.GetLogger().DebugFormat("Forwarding request {0} to {1}: {2}", context.Request.Uri.PathAndQuery, notificationPortForward, response);
                            }
                        }
                    }
                }

                await next();
            });
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



        private static void ConfigureDaemons()
        {
            DaemonSettings daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
            if (daemonConfig != null)
            {
                if (daemonConfig.Enabled)
                {
                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        try
                        {
                            if (daemon.Enabled)
                            {
                                Type type = Type.GetType(daemon.InitClass, true);
                                Daemon obj = Activator.CreateInstance(type) as Daemon;
                                if (obj == null)
                                    throw new ArgumentException(
                                        string.Format(
                                            "A daemon must implement IDaemon. Type '{0}' does not implement the interface.",
                                            type.Name));
                                obj.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.GetLogger().Error("Error initializing daemon '" + daemon.Name + "'.", e);
                        }
                    }
                }
            }
        }

        public async Task RegisterPluginActions()
        {
            var actionTemplateHosts = Utilities.FileUtils.LoadFileHostList();

            try
            {
                foreach (var url in actionTemplateHosts)
                {
                    var uri = url.StartsWith("http") ? url : "http://" + url;
                    uri += "/plugins/discover";

                    using (var client = new HttpClient())
                    using (var response = await client.GetAsync(uri))
                    using (var content = response.Content)
                    {
                        // For discover serialization see:
                        //   # pluginAzureSqlServer.Controllers.PluginController#DiscoverPlugins()
                        //   # pluginDockyardCore.Controllers.PluginController#DiscoverPlugins()
                        //   # pluginDocuSign.Controllers.PluginController#DiscoverPlugins()
                        var data = await content.ReadAsStringAsync();
                        var curActionList = JsonConvert.DeserializeObject<List<ActivityTemplateDO>>(data);

                        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                            PluginDO curPlugin = null;

                            foreach (var curItem in curActionList)
                            {
                                // Create only one plugin record.
                                if (curPlugin == null)
                                {
                                    uow.PluginRepository.Add(curItem.Plugin);
                                    curPlugin = curItem.Plugin;
                                }

                                // Check that ActivityTemplate with specified plugin does not exist yet.
                                var curTemplateExists = uow.ActivityTemplateRepository
                                    .GetQuery()
                                    .Any(x => x.Name == curItem.Name && x.Plugin.Name == curItem.Plugin.Name);

                                // If it doesn't exist, then create one.
                                if (!curTemplateExists)
                                {
                                    // Reassign Plugin to single plugin instance.
                                    curItem.Plugin = curPlugin;
                                    uow.ActivityTemplateRepository.Add(curItem);
                                }
                            }

                            uow.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorFormat("Error register plugins action template: {0} ", ex.Message);
            }
        }

        /// <summary>
        /// Loads Local Action Lists
        /// </summary>
        public void LoadLocalActionLists()
        {
            try
            {
                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ActivityTemplateRepository activityTemplateRepositary = uow.ActivityTemplateRepository;
                    List<ActivityTemplateDO> activityTemplateRepositaryItems = activityTemplateRepositary.GetAll().ToList();

                    if (!CheckForActivityTemplate("Extract From DocuSign Envelopes Into Azure Sql Server"))
                    {
                        ComponentActivitiesDTO componentActivitiesDTO = new ComponentActivitiesDTO();
                        componentActivitiesDTO.ComponentActivities = new List<ActivityTemplateDO>();

                        activityTemplateRepositaryItems = activityTemplateRepositary.GetAll().ToList();

                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Wait_For_DocuSign_Event"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                           (item => item.Name == "FilterUsingRunTimeData"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Extract_From_DocuSign_Envelope"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                          (item => item.Name == "MapFields"));

                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Write_To_Sql_Server"));

                        ActivityTemplateDO activityTemplate = new ActivityTemplateDO("Extract From DocuSign Envelopes Into Azure Sql Server", "localhost:46281", 1);
                        activityTemplate.ComponentActivities = (new JsonPackager().Pack(componentActivitiesDTO.ComponentActivities));
                        activityTemplate.Plugin = uow.PluginRepository.FindOne(x => x.Name == "pluginAzureSqlServer");

                        activityTemplateRepositary.Add(activityTemplate);
                        uow.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error in LoadLocalActionLists Method ", e);
            }
        }


        public bool CheckForActivityTemplate(string templateName)
        {
            bool found = true;
            try
            {               
                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ActivityTemplateRepository activityTemplateRepositary = uow.ActivityTemplateRepository;
                    List<ActivityTemplateDO> activityTemplateRepositaryItems = activityTemplateRepositary.GetAll().ToList();

                    if (activityTemplateRepositaryItems.Find(item => item.Name == templateName) == null)
                    {
                        found = false;
                    }
                    
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return found;
        }

    }
}
