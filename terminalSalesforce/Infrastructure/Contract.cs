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
    public class Contract : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var contractObject = (ContractDTO)salesforceObject;
            if (contractObject == null || string.IsNullOrEmpty(contractObject.AccountId))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select AccountId, ContractNumber from Contract";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultContracts = new List<ContractDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultContracts.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<ContractDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultContracts.Select(
                    contract =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountId", Value = contract.AccountId},
                                    new FieldDTO {Key = "ContractNumber", Value = contract.ContractNumber}
                                }
                        }));

            return payloads;
        }
    }
}