using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hub.Managers;
using StructureMap;
using terminalSalesforce.Services;

namespace terminalSalesforce.Infrastructure
{
    public class Lead
    {
        ForceClient client;       
        private ICrateManager _crateManager;


        public Lead()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }


        public async Task CreateLead(ActionDO currentActionDO, AuthorizationTokenDO authTokenDO)
        {
            
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);
            LeadDTO lead = new LeadDTO();


            var storage = _crateManager.GetStorage(currentActionDO);

            var curFieldList = storage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

            lead.FirstName = curFieldList.Controls.First(x => x.Name == "firstName").Value;
            lead.LastName = curFieldList.Controls.First(x => x.Name == "lastName").Value;
            lead.Company = curFieldList.Controls.First(x => x.Name == "companyName").Value;
            if (!String.IsNullOrEmpty(lead.LastName) && !String.IsNullOrEmpty(lead.Company))
            {
                var newLeadId = await client.CreateAsync("Lead", lead);
            }
        }

        public async Task<IList<FieldDTO>> GetFields(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);

            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);
            var fieldsQueryResponse = (JObject)await client.DescribeAsync<object>("Lead");

            var objectFields = new List<FieldDTO>();

            JToken leadFields = null;
            if (fieldsQueryResponse.TryGetValue("fields", out leadFields) && leadFields is JArray)
            {
                objectFields.AddRange(
                    leadFields.Select(
                        a => new FieldDTO(a.Value<string>("name"), a.Value<string>("label"))
                        {
                            Availability = AvailabilityType.Configuration
                        }));
            }

            return objectFields;
        }

        public async Task<IList<LeadDTO>> GetByQuery(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string conditionQuery)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);


            if (string.IsNullOrEmpty(conditionQuery))
            {
                conditionQuery = "select Id, FirstName, LastName, Company, Title from Lead";
            }
            else
            {
                conditionQuery = "select Id, FirstName, LastName, Company, Title from Lead where " + conditionQuery;
            }

            var response = await client.QueryAsync<object>(conditionQuery);

            var resultLeads = new List<LeadDTO>();

            if (response.Records.Count > 0)
            {
                resultLeads.AddRange(
                    response.Records.Select(record => ((JObject)record).ToObject<LeadDTO>()));
            }

            return resultLeads;
        }

        public void ParseAuthToken(string authonTokenAdditionalValues,out string instanceUrl,out string apiVersion)
        {
            int startIndexOfInstanceUrl = authonTokenAdditionalValues.IndexOf("instance_url");
            int startIndexOfApiVersion = authonTokenAdditionalValues.IndexOf("api_version");
            instanceUrl = authonTokenAdditionalValues.Substring(startIndexOfInstanceUrl, (startIndexOfApiVersion - 1 - startIndexOfInstanceUrl));
            apiVersion = authonTokenAdditionalValues.Substring(startIndexOfApiVersion, authonTokenAdditionalValues.Length - startIndexOfApiVersion);
            instanceUrl = instanceUrl.Replace("instance_url=", "");
            apiVersion = apiVersion.Replace("api_version=", "");
        }
    }
}