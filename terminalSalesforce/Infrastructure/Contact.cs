using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Contact : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            var contactObject = (ContactDTO) salesforceObject;

            if (contactObject == null || string.IsNullOrEmpty(contactObject.LastName))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            return "select FirstName, LastName, MobilePhone, Email from Contact";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultContacts = new List<ContactDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultContacts.AddRange(
                    queryResult.Records.Select(record => ((JObject) record).ToObject<ContactDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultContacts.Select(
                    contact =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "FirstName", Value = contact.FirstName},
                                    new FieldDTO {Key = "LastName", Value = contact.LastName},
                                    new FieldDTO {Key = "MobilePhone", Value = contact.MobilePhone},
                                    new FieldDTO {Key = "Email", Value = contact.Email}
                                }
                        }));

            return payloads;
        }
    }
}