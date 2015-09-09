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
            /*
             * TODO: This Plugin registration logic should be changed in V2
             */


            //IEnumerable<BasePluginRegistration> plugins = typeof(BasePluginRegistration)
            //    .Assembly.GetTypes()
            //    .Where(t => t.IsSubclassOf(typeof(BasePluginRegistration)) && !t.IsAbstract)
            //    .Select(t => (BasePluginRegistration)Activator.CreateInstance(t));
            //foreach (var plugin in plugins)
            //{
            //    plugin.RegisterActions();
            //}
        }

        /// <summary>
        /// Loads Local Action Lists
        /// </summary>
        public void LoadLocalActionLists()
        {
            using(IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            { 
                ActivityTemplateRepository activityTemplateRepositary = uow.ActionTemplateRepository;
                List<ActivityTemplateDO> activityTemplateRepositaryItems = activityTemplateRepositary.GetAll().ToList();

                if(activityTemplateRepositaryItems.Find(item=>item.Name=="Extract From DocuSign Envelopes Into Azure Sql Server")==null)
                { 

                List<ComponentActivitiesDTO> componentActivities = new List<ComponentActivitiesDTO>();               

                ComponentActivitiesDTO componentActivityOne = new ComponentActivitiesDTO();
                componentActivityOne.Id = 1;
                componentActivityOne.Name = "Wait for notification that an envelope has arrived at DocuSign";
                componentActivityOne.Version = "1";
                componentActivityOne.DefaultEndPoint = "AzureSqlServerPluginRegistration_v1";
                componentActivities.Add(componentActivityOne);
                
                ComponentActivitiesDTO componentActivityTwo = new ComponentActivitiesDTO();
                componentActivityTwo.Id = 2;
                componentActivityTwo.Name = "Filter the Envelope against some Criteria";
                componentActivityTwo.Version = "1";
                componentActivityTwo.DefaultEndPoint = "AzureSqlServerPluginRegistration_v1";
                componentActivities.Add(componentActivityTwo);

                ComponentActivitiesDTO componentActivityThree = new ComponentActivitiesDTO();
                componentActivityThree.Id = 3;
                componentActivityThree.Name = "Extract Data from the Envelope";
                componentActivityThree.Version = "1";
                componentActivityThree.DefaultEndPoint = "AzureSqlServerPluginRegistration_v1";
                componentActivities.Add(componentActivityThree);

                ComponentActivitiesDTO componentActivityFour = new ComponentActivitiesDTO();
                componentActivityFour.Id = 4;
                componentActivityFour.Name = "Map the Data to Target Fields";
                componentActivityFour.Version = "1";
                componentActivityFour.DefaultEndPoint = "AzureSqlServerPluginRegistration_v1";
                componentActivities.Add(componentActivityFour);

                ComponentActivitiesDTO componentActivityFive = new ComponentActivitiesDTO();
                componentActivityFive.Id = 5;
                componentActivityFive.Name = "Write the Data to AzureSqlServer";
                componentActivityFive.Version = "1";
                componentActivityFive.DefaultEndPoint = "AzureSqlServerPluginRegistration_v1";
                componentActivities.Add(componentActivityFive);

                ActivityTemplateDO activityTemplate = new ActivityTemplateDO("Extract From DocuSign Envelopes Into Azure Sql Server", "AzureSqlServerPluginRegistration_v1","1");             
                activityTemplate.ComponentActivities = (new JsonPackager().Pack(componentActivities));
                
                    activityTemplateRepositary.Add(activityTemplate);
                    uow.SaveChanges();

                }
            }
        }
    }
}
