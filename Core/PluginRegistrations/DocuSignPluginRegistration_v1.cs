using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public class DocuSignPluginRegistration_v1 : BasePluginRegistration
    {
        //public const string baseUrl = "plugin_docusign";
        //private readonly ActionNameListDTO availableActions;
        // private const string availableActions = @"[{ ""ActionType"" : ""Write to Sql Server"" , ""Version"": ""1.0"", ""ParentPluginRegistration"": ""AzureSql""}]";
        //private const string availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}]";

        public const string PluginRegistrationName = "DocuSign";

#if DEBUG
        public const string baseUrl = "http://localhost:53234/plugin_docusign";
        // public const string baseUrl = "http://ipv4.fiddler:53234/plugin_docusign";
#else
        public const string baseUrl = "http://services.dockyard.company/docusign/v1";
#endif
        public DocuSignPluginRegistration_v1()
            : base(InitAvailableActions(), baseUrl, PluginRegistrationName)
        {

        }

        private static ActionNameListDTO InitAvailableActions()
        {
            ActionNameListDTO curActionNameList = new ActionNameListDTO();
            ActionNameDTO curActionName = new ActionNameDTO();

            curActionName.ActionType = "Configure";
            curActionName.Version = "1";
            curActionNameList.ActionNames.Add(curActionName);

            curActionName = new ActionNameDTO();

            curActionName.ActionType = "Activate";
            curActionName.Version = "1";
            curActionNameList.ActionNames.Add(curActionName);

            curActionName = new ActionNameDTO();

            curActionName.ActionType = "Execute";
            curActionName.Version = "1";
            curActionNameList.ActionNames.Add(curActionName);

            return curActionNameList;
        }

//        public string GetConfigurationSettings(ActionTemplateDO curActionTemplateDo)
//        {
//            return @"
//                {""fields"":
//                    [{
//                        ""type"": ""textField"",
//                        ""name"": ""connection_string"",
//                        ""required"": true,
//                        ""fieldLabel"": ""SQL Connection String"",
//                        ""value"": """",
//                     }]
//                }";
//        }

        //public async override Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction)
        //{
           
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(baseUrl);
        //        client.DefaultRequestHeaders.Accept.Clear();

        //        var curActionDto = new ActionDesignDTO();
        //        Mapper.Map(curAction, curActionDto);

        //        var contentPost = new StringContent(JsonConvert.SerializeObject(curActionDto), Encoding.UTF8, "application/json");
        //        var response = await client.PostAsync(baseUrl + "/actions/Write_To_Sql_Server/field_mappings", contentPost).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

        //        var curMappingTargets = await response.Content.ReadAsStringAsync();
        //        var curResultJson = JArray.Parse(curMappingTargets.Replace("\\\"", "'").Replace("\"", "")).Select(t => t.ToString());

        //        return curResultJson;
        //    }
        //}
    }
}
