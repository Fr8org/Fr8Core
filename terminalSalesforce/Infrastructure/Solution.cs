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
    public class Solution : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var solutionObject = (SolutionDTO)salesforceObject;
            if (solutionObject == null || string.IsNullOrEmpty(solutionObject.SolutionName))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select SolutionName, SolutionNumber, Status from Solution";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultSolutions = new List<SolutionDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultSolutions.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<SolutionDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultSolutions.Select(
                    solution =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "SolutionName", Value = solution.SolutionName},
                                    new FieldDTO {Key = "SolutionNumber", Value = solution.SolutionNumber},
                                    new FieldDTO {Key = "Status", Value = solution.Status}
                                }
                        }));

            return payloads;
        }
    }
}