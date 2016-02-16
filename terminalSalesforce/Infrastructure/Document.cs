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
    public class Document : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var documentObject = (DocumentDTO)salesforceObject;
            if (documentObject == null || string.IsNullOrEmpty(documentObject.Name))
            {
                return false;
            }

            return true;
        }

        protected override string GetSelectAllQuery()
        {
            //return the query to select all accounts
            return "select Name, Body, FolderId from Document";
        }

        protected override IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultDocuments = new List<DocumentDTO>();

            if (queryResult.Records.Count > 0)
            {
                resultDocuments.AddRange(
                    queryResult.Records.Select(record => ((JObject)record).ToObject<DocumentDTO>()));
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(
                resultDocuments.Select(
                    document =>
                        new PayloadObjectDTO
                        {
                            PayloadObject =
                                new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "Body", Value = document.Body},
                                    new FieldDTO {Key = "Name", Value = document.Name},
                                    new FieldDTO {Key = "FolderId", Value = document.FolderId}
                                }
                        }));

            return payloads;
        }
    }
}