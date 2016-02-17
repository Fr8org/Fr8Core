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
    public class Case : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var caseObject = (CaseDTO)salesforceObject;
            if (caseObject == null || string.IsNullOrEmpty(caseObject.AccountId))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select AccountId, CaseNumber, CreatorName from Case";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultCases = new List<CaseDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultCases.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<CaseDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultCases.Select(
                    cases =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountId", Value = cases.AccountId},
                                    new FieldDTO {Key = "CaseNumber", Value = cases.CaseNumber},
                                    new FieldDTO {Key = "CreatorName", Value = cases.CreatorName}
                                }
                        }));

            return payloads;
        }
    }
}