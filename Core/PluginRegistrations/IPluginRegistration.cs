using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; set; }

        void RegisterActions();

        string CallPluginRegistrationByString(string typeName, string methodName, ActionRegistrationDO curActionRegistrationDO);

        IEnumerable<ActionRegistrationDO> AvailableActions { get; }
		
      //  JObject GetConfigurationSettings();

        string AssembleName(ActionRegistrationDO curActionRegistrationDO);
        Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction);
    }
}
