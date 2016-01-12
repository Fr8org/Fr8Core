using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using StructureMap;

namespace terminalSalesforce.Infrastructure
{
    public abstract class SalesforceObject
    {
        public async Task<SuccessResponse> Create<T>(T salesforceObject, string salesforceObjectName, ForceClient forceClient)
        {
            SuccessResponse successResponse = null;
            if (ValidateObject(salesforceObject))
            {
                successResponse = await forceClient.CreateAsync(salesforceObjectName, salesforceObject);
            }

            return successResponse ?? new SuccessResponse(); //TODO: Vas, check the response for the error, success and id param and change this.
        }

        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, ForceClient forceClient)
        {
            var fieldsQueryResponse = (JObject)await forceClient.DescribeAsync<object>(salesforceObjectName);

            var objectFields = new List<FieldDTO>();

            JToken leadFields;
            if (fieldsQueryResponse.TryGetValue("fields", out leadFields) && leadFields is JArray)
            {
                objectFields.AddRange(
                    leadFields.Select(a => new FieldDTO(a.Value<string>("name"), a.Value<string>("label"))));
            }

            return objectFields;
        }

        public async Task<IList<PayloadObjectDTO>> GetByQuery(string conditionQuery, ForceClient forceClient)
        {
            var selectQuery = GetSelectAllQuery();

            if (!string.IsNullOrEmpty(conditionQuery))
            {
                selectQuery += " where " + conditionQuery;
            }

            var response = await forceClient.QueryAsync<object>(selectQuery);

            return ParseQueryResult(response);
        }

        protected abstract bool ValidateObject(object salesforceObject);

        protected abstract string GetSelectAllQuery();

        protected abstract IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult);
    }
}