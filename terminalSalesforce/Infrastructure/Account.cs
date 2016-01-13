using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Account : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var accountObject = (AccountDTO) salesforceObject;
            if (accountObject == null || string.IsNullOrEmpty(accountObject.Name))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select Name, AccountNumber, Phone from Account";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultAccounts = new List<AccountDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultAccounts.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<AccountDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultAccounts.Select(
                    account =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountNumber", Value = account.AccountNumber},
                                    new FieldDTO {Key = "Name", Value = account.Name},
                                    new FieldDTO {Key = "Phone", Value = account.Phone}
                                }
                        }));

            return payloads;
        }
    }
}