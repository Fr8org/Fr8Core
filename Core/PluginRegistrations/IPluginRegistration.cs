using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; set; }

        IEnumerable<ActionRegistrationDO> AvailableActions { get; }

        void RegisterActions();
		
        JObject GetConfigurationSettings();

        Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction);
    }
}