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
    public class Order : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var orderObject = (OrderDTO)salesforceObject;
            if (orderObject == null || string.IsNullOrEmpty(orderObject.AccountId))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select AccountId, OwnerId, Pricebook2Id from Order";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultOrders = new List<OrderDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultOrders.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<OrderDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultOrders.Select(
                    order =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountId", Value = order.AccountId},
                                    new FieldDTO {Key = "OwnerId", Value = order.OwnerId},
                                    new FieldDTO {Key = "Pricebook2Id", Value = order.Pricebook2Id}
                                }
                        }));

            return payloads;
        }
    }
}