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

        public async Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri)
        //    public IList<ActivityTemplateDO> GetAvailableActions(string uri)
        {
            // IList<ActionTemplateDO> actionTemplateList = null; ;
            var restClient = new RestfulServiceClient();
            return await restClient.GetAsync<IList<ActivityTemplateDO>>(new Uri(uri, UriKind.Absolute));
            //var actionTemplateList = restClient.GetAsync<Task<IList<ActivityTemplateDO>>>(new Uri(uri, UriKind.Absolute)).Result;
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
    }
}
