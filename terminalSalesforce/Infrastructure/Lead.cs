using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Lead : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            var leadObject = (LeadDTO) salesforceObject;

            if (leadObject == null || string.IsNullOrEmpty(leadObject.LastName) ||
                string.IsNullOrEmpty(leadObject.Company))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            return "select Id, FirstName, LastName, Company, Title from Lead";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultLeads = new List<LeadDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultLeads.AddRange(
                    queryResult.Records.Select(record => ((JObject) record).ToObject<LeadDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultLeads.Select(
                    lead =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "Id", Value = lead.Id},
                                    new FieldDTO {Key = "FirstName", Value = lead.FirstName},
                                    new FieldDTO {Key = "LastName", Value = lead.LastName},
                                    new FieldDTO {Key = "Company", Value = lead.Company},
                                    new FieldDTO {Key = "Title", Value = lead.Title}
                                }
                        }));

            return payloads;
        }
    }
}