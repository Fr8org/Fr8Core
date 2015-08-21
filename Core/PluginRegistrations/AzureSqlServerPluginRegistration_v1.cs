using Data.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace Core.PluginRegistrations
{
    public class AzureSqlServerPluginRegistration_v1 : BasePluginRegistration
    {
       // private readonly ActionNameListDTO availableActions;
        //private const string availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}]";
#if DEBUG
        public const string baseUrl = "http://localhost:23432";
#else
        public const string baseUrl = "http://services.dockyard.company/azure_sql_server/v1";
#endif
        public AzureSqlServerPluginRegistration_v1()
            : base(InitAvailableActions(), baseUrl)
        {

        }

        private static ActionNameListDTO InitAvailableActions()
        {
            ActionNameListDTO curActionNameList = new ActionNameListDTO();
            ActionNameDTO curActionName = new ActionNameDTO();

            curActionName.ActionType = "Write";
            curActionName.Version = "1";
            curActionNameList.ActionNames.Add(curActionName);
            return curActionNameList;
        }

        public string GetConfigurationSettings(ActionRegistrationDO curActionRegistrationDO)
        {
            return "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
        }

        public async override Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction)
        {
            List<string> result;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();

                var curActionDto = new ActionDTO();
                Mapper.Map(curAction, curActionDto);

                var contentPost = new StringContent(JsonConvert.SerializeObject(curActionDto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl + "/actions/write_to_sql_server/field_mappings", contentPost).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

                var curMappingTargets = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<List<string>>(curMappingTargets);
            }
            return result;
        }
    }
}
