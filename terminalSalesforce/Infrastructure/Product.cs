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
    public class Product : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var productObject = (ProductDTO)salesforceObject;
            if (productObject == null || string.IsNullOrEmpty(productObject.Name))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select CurrencyIsoCode, Name, DefaultPrice from Product";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultproducts = new List<ProductDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultproducts.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<ProductDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultproducts.Select(
                    product =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "CurrencyIsoCode", Value = product.CurrencyIsoCode},
                                    new FieldDTO {Key = "Name", Value = product.Name},
                                    new FieldDTO {Key = "DefaultPrice", Value = product.DefaultPrice}
                                }
                        }));

            return payloads;
        }
    }
}