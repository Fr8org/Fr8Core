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
    public class SalesforceObject
    {
        /// <summary>
        /// Creates a Salesforce object
        /// </summary>
        public async Task<SuccessResponse> Create<T>(T salesforceObject, string salesforceObjectName, ForceClient forceClient)
        {
            SuccessResponse successResponse = null;

            successResponse = await forceClient.CreateAsync(salesforceObjectName, salesforceObject);

            return successResponse ?? new SuccessResponse(); 
        }

        /// <summary>
        /// Gets fields of the given Salesforce object name.
        /// Please see https://developer.salesforce.com/docs/atlas.en-us.api.meta/api/sforce_api_calls_describesobjects_describesobjectresult.htm#topic-title
        /// </summary>
        public async Task<IList<FieldDTO>> GetFields(string salesforceObjectName, ForceClient forceClient, bool onlyUpdatableFields = false)
        {
            //Get the fields of the salesforce object name by calling Describe API
            var fieldsQueryResponse = (JObject)await forceClient.DescribeAsync<object>(salesforceObjectName);

            var objectFields = new List<FieldDTO>();

            //parse them into the list of FieldDTO
            JToken fieldDescriptions;
            if (fieldsQueryResponse.TryGetValue("fields", out fieldDescriptions) && fieldDescriptions is JArray)
            {
                //if asked to get only updatable fields, filter the fields which are updateable
                if (onlyUpdatableFields)
                {
                    fieldDescriptions = new JArray(fieldDescriptions.Where(fieldDescription => (fieldDescription.Value<bool>("updateable") == true)));
                }

                var fields = fieldDescriptions.Select(fieldDescription =>
                                    /*
                                    Select Fields as FieldDTOs with                                    

                                    Key -> Field Name
                                    Value -> Field Lable
                                    AvailabilityType -> Run Time
                                    FieldType -> Field Type

                                    IsRequired -> The Field is required when ALL the below conditions are true.
                                      nillable            = false, Meaning, the field must have a valid value. The field's value should not be NULL or NILL or Empty
                                      defaultedOnCreate   = false, Meaning, Salesforce itself does not assign default value for this field when object is created (ex. ID)
                                      updateable          = true,  Meaning, The filed's value must be updatable by the user. 
                                                                            User must be able to set or modify the value of this field.
                                    */
                                    new FieldDTO(fieldDescription.Value<string>("name"), fieldDescription.Value<string>("label"), Data.States.AvailabilityType.RunTime)
                                    {
                                        FieldType = fieldDescription.Value<string>("type"),

                                        IsRequired = fieldDescription.Value<bool>("nillable") == false &&
                                                     fieldDescription.Value<bool>("defaultedOnCreate") == false &&
                                                     fieldDescription.Value<bool>("updateable") == true

                                    }).OrderBy(field => field.Key);

                objectFields.AddRange(fields);
            }

            return objectFields;
        }

        /// <summary>
        /// Gets Salesforce objects based on query
        /// </summary>
        public async Task<IList<PayloadObjectDTO>> GetByQuery(string salesforceObjectName, IEnumerable<string> fields, string conditionQuery, ForceClient forceClient)
        {
            //get select all query for the object.
            var selectQuery = string.Format("select {0} from {1}", string.Join(", ", fields.ToList()), salesforceObjectName);

            //if condition query is not empty, add it to where clause
            if (!string.IsNullOrEmpty(conditionQuery))
            {
                selectQuery += " where " + conditionQuery;
            }

            var response = await forceClient.QueryAsync<object>(selectQuery);

            //parsing the query resonse is delegated to the derived classes.
            return ParseQueryResult(response);
        }

        private IList<PayloadObjectDTO> ParseQueryResult(QueryResult<object> queryResult)
        {
            var resultLeads = new List<JObject>();

            if (queryResult.Records.Count > 0)
            {
                resultLeads = queryResult.Records.Select(record => ((JObject)record)).ToList();
            }

            var payloads = new List<PayloadObjectDTO>();

            payloads.AddRange(resultLeads
                                .Select(l => new PayloadObjectDTO
                                    {
                                        PayloadObject = l.Properties()
                                                         .Where(p => !string.IsNullOrEmpty(p.Value.Value<object>().ToString()))
                                                         .Select(p => 
                                                            new FieldDTO {
                                                                Key = p.Name,
                                                                Value = p.Value.Value<object>().ToString(),
                                                                Availability = Data.States.AvailabilityType.RunTime
                                                            })
                                                            .ToList()
                                }));

            return payloads;
        }
    }
}