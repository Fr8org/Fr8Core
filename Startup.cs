using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Configuration;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Interfaces.DataTransferObjects;
using Microsoft.Owin;
using Microsoft.WindowsAzure;
using Owin;
using StructureMap;
using Utilities.Logging;
using Utilities;
using System.Threading.Tasks;
using System.IO;
using Utilities.Serializers.Json;
using Core.Services;
using Core.Managers;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();
            ConfigureAuth(app);

            RegisterPluginActions();

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

        public void RegisterPluginActions()
        {

            try
            {
                var activityTemplateHosts = Utilities.FileUtils.LoadFileHostList();
                int count = 0;
                foreach (string url in activityTemplateHosts)
                {
                    var uri = url.StartsWith("http") ? url : "http://" + url;
                    uri += "/actions/action_templates";

                    IList<ActivityTemplateDO> activityTemplateList = new Plugin().GetAvailableActions(uri).Result;
                    foreach (var template in activityTemplateList)
                    {
                        new ActivityTemplate().Register(template);
                        count++;
                    }
                }

                EventReporter alertReporter = ObjectFactory.GetInstance<EventReporter>();
                alertReporter.ActivityTemplatesSuccessfullyRegistered(count);
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

                        if (!CheckForActivityTemplate("Wait for notification that an envelope has arrived at DocuSign"))
                        {
                            ActivityTemplateDO componentActivityOne = new ActivityTemplateDO("Wait for notification that an envelope has arrived at DocuSign"
                                , "1", "localhost:46281", "localhost:46281");
                            activityTemplateRepositary.Add(componentActivityOne);
                        }
                        if (!CheckForActivityTemplate("Filter the Envelope against some Criteria"))
                        {
                            ActivityTemplateDO componentActivityTwo = new ActivityTemplateDO("Filter the Envelope against some Criteria", "1"
                             , "localhost:46281", "localhost:46281");
                            activityTemplateRepositary.Add(componentActivityTwo);
                        }
                        if (!CheckForActivityTemplate("Extract Data from the Envelope"))
                        {
                            ActivityTemplateDO componentActivityThree = new ActivityTemplateDO("Extract Data from the Envelope", "1"
                             , "localhost:46281", "localhost:46281");
                            activityTemplateRepositary.Add(componentActivityThree);
                        }
                        if (!CheckForActivityTemplate("Map the Data to Target Fields"))
                        {
                            ActivityTemplateDO componentActivityFour = new ActivityTemplateDO("Map the Data to Target Fields", "1"
                             , "localhost:46281", "localhost:46281");
                            activityTemplateRepositary.Add(componentActivityFour);
                        }
                        if (!CheckForActivityTemplate("Write the Data to AzureSqlServer"))
                        {
                            ActivityTemplateDO componentActivityFive = new ActivityTemplateDO("Write the Data to AzureSqlServer", "1"
                             , "localhost:46281", "localhost:46281");
                            activityTemplateRepositary.Add(componentActivityFive);
                        }
                        uow.SaveChanges();

                        activityTemplateRepositaryItems = activityTemplateRepositary.GetAll().ToList();

                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Wait for notification that an envelope has arrived at DocuSign"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                           (item => item.Name == "Filter the Envelope against some Criteria"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Extract Data from the Envelope"));


                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                          (item => item.Name == "Map the Data to Target Fields"));

                        componentActivitiesDTO.ComponentActivities.Add(activityTemplateRepositaryItems.Find
                            (item => item.Name == "Write the Data to AzureSqlServer"));

                        ActivityTemplateDO activityTemplate = new ActivityTemplateDO("Extract From DocuSign Envelopes Into Azure Sql Server", "1"
                             , "localhost:46281", "localhost:46281");
                        activityTemplate.ComponentActivities = (new JsonPackager().Pack(componentActivitiesDTO.ComponentActivities));

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
