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

        string CallPluginRegistrationByString(string typeName, string methodName, ActionTemplateDO curActionTemplateDo);

        IEnumerable<ActionTemplateDO> AvailableActions { get; }
		
      //  JObject GetConfigurationSettings();

        string AssembleName(ActionTemplateDO curActionTemplateDo);
        Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction);
    }
}
