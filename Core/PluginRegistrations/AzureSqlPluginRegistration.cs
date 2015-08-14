using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public class AzureSqlPluginRegistration : BasePluginRegistration
    {
        public const string baseUrl = "AzureSql.BaseUrl";
        private const string availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}]";

        public AzureSqlPluginRegistration(IAction action)
            : base(availableActions, baseUrl)
        {

        }

        public override JObject GetConfigurationSettings()
        {
            return null;
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
                var response = await client.PostAsync("/actions/write_to_sql_server/field_mappings", contentPost).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

                var curMappingTargets = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<List<string>>(curMappingTargets);
            }
            return result;
        }
    }
}