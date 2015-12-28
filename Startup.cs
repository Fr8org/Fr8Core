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

[assembly: OwinStartup(typeof(HubWeb.Startup))]

namespace HubWeb
{
    public partial class Startup
    {
        public async void Configuration(IAppBuilder app)
        {
            //ConfigureDaemons();
            ConfigureAuth(app);
            ConfigureHangfire(app, "DockyardDB");

            await RegisterTerminalActions();

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
                            Uri url = null;
                            foreach (var notificationPortForward in notificationPortForwards)
                            {
                                try
                                {
                                    url = new Uri(string.Concat("http://", notificationPortForward, context.Request.Uri.PathAndQuery));
                                    var response = await
                                        forwarder.PostAsync(
                                            url,
                                            new StreamContent(context.Request.Body));
                                    Logger.GetLogger().DebugFormat("Forwarding request {0} to {1}: {2}", context.Request.Uri.PathAndQuery, notificationPortForward, response);
                                }
                                catch (TaskCanceledException)
                                {
                                    //Timeout
                                    throw new TimeoutException(
                                        String.Format("Timeout while making HTTP request.  \r\nURL: {0},   \r\nMethod: {1}",
                                       url.ToString(),
                                        HttpMethod.Post.Method));
                                }
                            }
                        }
                    }
                }

                await next();
            });
        }

        public void ConfigureHangfire(IAppBuilder app, string connectionString)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            app.UseHangfireDashboard();
            app.UseHangfireServer();
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
            var listRegisteredActivityTemplates = new List<ActivityTemplateDO>();
            var alertReporter = ObjectFactory.GetInstance<EventReporter>();

            var activityTemplateHosts = Utilities.FileUtils.LoadFileHostList();
            int count = 0;
            var uri = string.Empty;
            foreach (string url in activityTemplateHosts)
            {
                try
                {
                    uri = url.StartsWith("http") ? url : "http://" + url;
                    uri += "/terminals/discover";

                    var terminalService = ObjectFactory.GetInstance<ITerminal>(); ;
                    var activityTemplateList = await terminalService.GetAvailableActions(uri);
                    foreach (var curItem in activityTemplateList)
                    {
                        try
                        {
                            new ActivityTemplate().Register(curItem);
                            listRegisteredActivityTemplates.Add(curItem);
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

            UpdateActivityTemplates(listRegisteredActivityTemplates);
            alertReporter.ActivityTemplatesSuccessfullyRegistered(count);
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

                    if (!activityTemplateRepositaryItems.Any(item => item.Name == templateName))
                    {
                        found = false;
                    }

                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error(String.Format("Error checking for activity template \"{0}\"", templateName), e);
            }
            return found;
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<Startup>(url: url);
        }

        private void UpdateActivityTemplates(List<ActivityTemplateDO> listRegisteredItems)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var needSave = false;
                var repository = uow.ActivityTemplateRepository;
                var listRepositaryItems = repository.GetAll().ToList();
                foreach (var repositaryItem in listRepositaryItems)
                {
                    var registeredItem = listRegisteredItems.FirstOrDefault(x => x.Name.ToLower().Equals(repositaryItem.Name.ToLower()));
                    if (!object.Equals(registeredItem, default(ActivityTemplateDO)))
                    {
                        if (repositaryItem.Label != repositaryItem.Label)
                        {
                            repositaryItem.Label = registeredItem.Label;
                            needSave = true;
                        }

                        if (repositaryItem.MinPaneWidth != registeredItem.MinPaneWidth)
                        {
                            repositaryItem.MinPaneWidth = registeredItem.MinPaneWidth;
                            needSave = true;
                        }

                        if (registeredItem.WebServiceId != null &&
                            repositaryItem.WebServiceId != registeredItem.WebServiceId)
                        {
                            repositaryItem.WebServiceId = registeredItem.WebServiceId;
                            needSave = true;
                        }

                        if (registeredItem.TerminalId > 0 &&
                            repositaryItem.TerminalId != registeredItem.TerminalId)
                        {
                            repositaryItem.TerminalId = registeredItem.TerminalId;
                            needSave = true;
                        }

                        if (repositaryItem.Version != registeredItem.Version)
                        {
                            repositaryItem.Version = registeredItem.Version;
                            needSave = true;
                        }

                        if (repositaryItem.Category != registeredItem.Category)
                        {
                            repositaryItem.Category = registeredItem.Category;
                            needSave = true;
                        }

                        // if (repositaryItem.AuthenticationType != registeredItem.AuthenticationType)
                        // {
                        //     repositaryItem.AuthenticationType = registeredItem.AuthenticationType;
                        //     needSave = true;
                        // }

                        if (repositaryItem.ComponentActivities != registeredItem.ComponentActivities)
                        {
                            repositaryItem.ComponentActivities = registeredItem.ComponentActivities;
                            needSave = true;
                        }

                        if (repositaryItem.Tags != registeredItem.Tags)
                        {
                            repositaryItem.Tags = registeredItem.Tags;
                            needSave = true;
                        }

                        if (repositaryItem.Description != registeredItem.Description)
                        {
                            repositaryItem.Description = registeredItem.Description;
                            needSave = true;
                        }

                    }
                }

                if (needSave)
                {
                    uow.SaveChanges();
                }
            }
        }

    }
}
