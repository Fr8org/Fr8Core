
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;

namespace Core.Services
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

        public string Authorize()
        {
            return "AuthorizationToken";
        }

        public async Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri)
        //    public IList<ActivityTemplateDO> GetAvailableActions(string uri)
        {
            // IList<ActionTemplateDO> actionTemplateList = null; ;
            var restClient = new RestfulServiceClient();
            return await restClient.GetAsync<IList<ActivityTemplateDO>>(new Uri(uri, UriKind.Absolute));
            //var actionTemplateList = restClient.GetAsync<Task<IList<ActivityTemplateDO>>>(new Uri(uri, UriKind.Absolute)).Result;
        }

        public string GetPluginUrl(string curPluginName, string curPluginVersion)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IPluginDO curPlugin =
                    uow.PluginRepository.FindOne(
                        plugin => plugin.Name.Equals(curPluginName) && plugin.Version.Equals(curPluginVersion));

                return (curPlugin != null) ? curPlugin.Endpoint : string.Empty;
            }
        }
    }
}
