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
    public class Contact
    {
        private readonly ICrateManager _crateManager;
        ForceClient client;       

        public Contact()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }


        public async Task CreateContact(ActionDO currentActionDO, AuthorizationTokenDO authTokenDO)
        {

            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);
            ContactDTO contact = new ContactDTO();
            var curFieldList = _crateManager.GetStorage(currentActionDO.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().First();
               
            contact.FirstName = curFieldList.Controls.First(x => x.Name == "firstName").Value;
            contact.LastName = curFieldList.Controls.First(x => x.Name == "lastName").Value;
            contact.MobilePhone = curFieldList.Controls.First(x => x.Name == "mobilePhone").Value;
            contact.Email = curFieldList.Controls.First(x => x.Name == "email").Value;
            if (!String.IsNullOrEmpty(contact.LastName))
            {
                var contactId = await client.CreateAsync("Contact", contact);
            }
        }

        public async Task<IList<FieldDTO>> GetFields(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);

            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);
            var fieldsQueryResponse = (JObject)await client.DescribeAsync<object>("Contact");

            var objectFields = new List<FieldDTO>();

            JToken contactFields = null;
            if (fieldsQueryResponse.TryGetValue("fields", out contactFields) && contactFields is JArray)
            {
                objectFields.AddRange(
                    contactFields.Select(a => new FieldDTO(a.Value<string>("name"), a.Value<string>("label"))));
            }

            return objectFields;
        }

        public async Task<IList<ContactDTO>> GetByQuery(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string conditionQuery)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);


            if (string.IsNullOrEmpty(conditionQuery))
            {
                conditionQuery = "select FirstName, LastName, MobilePhone, Email from Contact";
            }
            else
            {
                conditionQuery = "select FirstName, LastName, MobilePhone, Email from Contact where " + conditionQuery;
            }

            var response = await client.QueryAsync<object>(conditionQuery);

            var resultContacts = new List<ContactDTO>();

            if (response.Records.Count > 0)
            {
                resultContacts.AddRange(
                    response.Records.Select(record => ((JObject)record).ToObject<ContactDTO>()));
            }

            return resultContacts;
        }

        public void ParseAuthToken(string authonTokenAdditionalValues, out string instanceUrl, out string apiVersion)
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