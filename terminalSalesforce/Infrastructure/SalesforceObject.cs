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
        /// <summary>
        /// Creates a Salesforce object
        /// </summary>
        public async Task<SuccessResponse> Create<T>(T salesforceObject, string salesforceObjectName, ForceClient forceClient)
        {
            SuccessResponse successResponse = null;

            //if the given object is valid, create. Validation is delegated to the derived classes.
            if (ValidateObject(salesforceObject))
            {
                successResponse = await forceClient.CreateAsync(salesforceObjectName, salesforceObject);
            }

            return successResponse ?? new SuccessResponse(); 
        }

        /// <summary>
        /// Gets fields of the given Salesforce object name
        /// </summary>
        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, ForceClient forceClient)
        {
            //Get the fields of the salesforce object name by calling Describe API
            var fieldsQueryResponse = (JObject)await forceClient.DescribeAsync<object>(salesforceObjectName);

            var objectFields = new List<FieldDTO>();

            //parse them into the list of FieldDTO
            JToken leadFields;
            if (fieldsQueryResponse.TryGetValue("fields", out leadFields) && leadFields is JArray)
            {
                objectFields.AddRange(
                    leadFields.Select(a => new FieldDTO(a.Value<string>("name"), a.Value<string>("label"), Data.States.AvailabilityType.RunTime)));
            }

            return objectFields;
        }

        /// <summary>
        /// Gets Salesforce objects based on query
        /// </summary>
        public async Task<IList<PayloadObjectDTO>> GetByQuery(string conditionQuery, ForceClient forceClient)
        {
            //get select all query for the object. Delegated to derived classes
            var selectQuery = GetSelectAllQuery();

            //if condition query is not empty, add it to where clause
            if (!string.IsNullOrEmpty(conditionQuery))
            {
                selectQuery += " where " + conditionQuery;
            }

            var response = await forceClient.QueryAsync<object>(selectQuery);

            //parsing the query resonse is delegated to the derived classes.
            return ParseQueryResult(response);
        }

        protected abstract bool ValidateObject(object salesforceObject);

        protected abstract string GetSelectAllQuery();

        protected abstract IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult);
    }
}