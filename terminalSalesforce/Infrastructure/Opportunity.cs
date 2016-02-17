using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class Opportunity : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var opportunityObject = (OpportunityDTO)salesforceObject;
            if (opportunityObject == null || string.IsNullOrEmpty(opportunityObject.Name))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select Name, AccountId, StageName, CloseDate from Opportunity";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultOpportunity = new List<OpportunityDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultOpportunity.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<OpportunityDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultOpportunity.Select(
                    opportunity =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountId", Value = opportunity.AccountId},
                                    new FieldDTO {Key = "Name", Value = opportunity.Name},
                                    new FieldDTO {Key = "StageName", Value = opportunity.StageName},
                                    new FieldDTO {Key = "CloseDate", Value = opportunity.CloseDate}
                                }
                        }));

            return payloads;
        }
    }
}