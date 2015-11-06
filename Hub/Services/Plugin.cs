using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.Manifests;
using Hub.Managers;

namespace Hub.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class Plugin : IPlugin
    {
        public IEnumerable<PluginDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PluginRepository.GetAll();
            }
        }

        public async Task<IList<string>> RegisterTerminals(string uri)
        {            
            var eventReporter = ObjectFactory.GetInstance<EventReporter>();

            var activityTemplateList = await GetAvailableActions(uri);

            List<string> activityTemplateNames = new List<string>(); 
            foreach (var activityTemplate in activityTemplateList)
            {
                try
                {
                    new ActivityTemplate().Register(activityTemplate);
                    activityTemplateNames.Add(activityTemplate.Name);
                }
                catch (Exception ex)
                {
                    eventReporter = ObjectFactory.GetInstance<EventReporter>();
                    eventReporter.ActivityTemplatePluginRegistrationError(
                        string.Format("Failed to register {0} terminal. Error Message: {1}", activityTemplate.Plugin.Name, ex.Message),
                        ex.GetType().Name);
                }
            }

            return activityTemplateNames;
        }
        

        /// <summary>
        /// Parses the required plugin service URL for the given action by Plugin Name and its version
        /// </summary>
        /// <param name="curPluginName">Name of the required plugin</param>
        /// <param name="curPluginVersion">Version of the required plugin</param>
        /// <param name="curActionName">Required action</param>
        /// <returns>Parsed URl to the plugin for its action</returns>
        public string ParsePluginUrlFor(string curPluginName, string curPluginVersion, string curActionName)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //get the plugin by name and version
                IPluginDO curPlugin =
                    uow.PluginRepository.FindOne(
                        plugin => plugin.Name.Equals(curPluginName) && plugin.Version.Equals(curPluginVersion));


                string curPluginUrl = string.Empty;

                //if there is a valid plugin, prepare the URL with its endpoint and add the given action name
                if (curPlugin != null)
                {
                    curPluginUrl += @"http://" + curPlugin.Endpoint + "/" + curActionName;
                }

                //return the pugin URL
                return curPluginUrl;
            }
        }

        private async Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri)
        {
            var restClient = new RestfulServiceClient();
            var standardFr8TerminalCM = await restClient.GetAsync<StandardFr8TerminalCM>(new Uri(uri, UriKind.Absolute));
            return standardFr8TerminalCM.Actions;
        }
    }
}
