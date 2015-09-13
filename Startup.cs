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
using Microsoft.Owin;
using Microsoft.WindowsAzure;
using Owin;
using StructureMap;
using Utilities.Logging;
using Utilities;
using System.Threading.Tasks;
using System.IO;
using Utilities.Serializers.Json;

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
            var path = Server.ServerPhysicalPath + "DockyardPlugins.txt";

            IList<string> urls = null;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    if (sr.Peek() < 0)
                        throw new ApplicationException("DockyardPlugins.txt is empty.");
                    urls = new List<string>();
                    while (sr.Peek() >= 0)
                    {
                        urls.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorFormat("Error register plugins actions: '{0}'", ex.Message);
                return; 
            }

            try
            {
                foreach (string url in urls)
                {
                    var uri = url.StartsWith("http") ? url : "http://" + url;
                    uri += "/actions/action_templates";

                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    using (HttpContent content = response.Content)
                    {
                        var data = await content.ReadAsStringAsync();
                        var actionList = new JsonSerializer().DeserializeList<ActionTemplateDO>(data);
                        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                            foreach (ActionTemplateDO item in actionList)
                            {
                                uow.ActionTemplateRepository.Add(item);
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
    }
}
