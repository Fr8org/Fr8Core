using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalSalesforce.Services;

namespace terminalSalesforce.Infrastructure
{
    public class Contact
    {
        ForceClient client;       
        public async Task CreateContact(ActionDTO currentActionDTO)
        {

            string instanceUrl, apiVersion;
            ParseAuthToken(currentActionDTO.AuthToken.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, currentActionDTO.AuthToken.Token, apiVersion);
            ContactDTO contact = new ContactDTO();
            var curFieldList =
               JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(currentActionDTO.CrateStorage.CrateDTO.First(field => field.Contents.Contains("firstName")).Contents);
            contact.FirstName = curFieldList.Controls.First(x => x.Name == "firstName").Value;
            contact.LastName = curFieldList.Controls.First(x => x.Name == "lastName").Value;
            contact.MobilePhone = curFieldList.Controls.First(x => x.Name == "mobilePhone").Value;
            contact.Email = curFieldList.Controls.First(x => x.Name == "email").Value;
            if (!String.IsNullOrEmpty(contact.LastName))
            {
                var contactId = await client.CreateAsync("Contact", contact);
            }
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