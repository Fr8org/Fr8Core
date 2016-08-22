using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Control;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.DataTransferObjects.Helpers
{
    public static class ActivityResponseHelper
    {
        #region Properties

        public const string ErrorPropertyName = "error";
        public const string PayloadPropertyName = "payload";
        public const string ResponseMessagePropertyName = "responseMessage";
        public const string ActivityPropertyName = "activity";
        public const string DocumentationPropertyName = "documentation";

        #endregion

        #region Base methods

        private static ActivityResponseDTO AddBaseDTO<T>(ActivityResponseDTO activityResponse, string propertyName, T objectToAdd )
        {
            var responseBody = string.IsNullOrEmpty(activityResponse.Body) 
                ? new JObject() : JObject.Parse(activityResponse.Body);

            //in future extend it to work with JArray-property base if needed
            JToken token = JsonConvert.SerializeObject(objectToAdd);
            
            JToken tempToken;
            if (responseBody.TryGetValue(propertyName, out tempToken))
            {
                //check if already some object exists, that case update it based on property
                responseBody[propertyName] = token;
            }
            else
            {
                responseBody.Add(propertyName, token);
            }

            activityResponse.Body = JsonConvert.SerializeObject(responseBody);

            return activityResponse;
        }

        private static bool TryParseBaseDTO<T>(ActivityResponseDTO activityResponse, string propertyName, out T parsedObject)
        {
            parsedObject = default(T);

            if(string.IsNullOrEmpty(activityResponse.Body))
                return false;

            try
            {
                JObject responseBody = JObject.Parse(activityResponse.Body);
                var tokenObj = responseBody[propertyName];
                if (tokenObj == null) return false;

                parsedObject = JsonConvert.DeserializeObject<T>(tokenObj.ToString());
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        #endregion

        #region Error Response Methods

        public static ActivityResponseDTO AddErrorDTO(this ActivityResponseDTO activityResponse, ErrorDTO errorDTO)
        {
            return AddBaseDTO<ErrorDTO>(activityResponse, ErrorPropertyName, errorDTO);
        }
        
        public static bool TryParseErrorDTO(this ActivityResponseDTO activityResponse, out ErrorDTO errorDTO)
        {
            return TryParseBaseDTO<ErrorDTO>(activityResponse, ErrorPropertyName, out errorDTO);
        }

        #endregion

        #region Success Response methods

        public static ActivityResponseDTO AddPayloadDTO(this ActivityResponseDTO activityResponse, PayloadDTO payloadDTO)
        {
            return AddBaseDTO<PayloadDTO>(activityResponse, PayloadPropertyName, payloadDTO);
        }

        public static bool TryParsePayloadDTO(this ActivityResponseDTO activityResponse, out PayloadDTO payloadDTO)
        {
            return TryParseBaseDTO<PayloadDTO>(activityResponse, PayloadPropertyName, out payloadDTO);
        }

        public static ActivityResponseDTO AddResponseMessageDTO(this ActivityResponseDTO activityResponse, ResponseMessageDTO responseMessageDTO)
        {
            return AddBaseDTO<ResponseMessageDTO>(activityResponse, ResponseMessagePropertyName, responseMessageDTO);
        }

        public static bool TryParseResponseMessageDTO(this ActivityResponseDTO activityResponse, out ResponseMessageDTO responseMessageDTO)
        {
            return TryParseBaseDTO<ResponseMessageDTO>(activityResponse, ResponseMessagePropertyName, out responseMessageDTO);
        }

        #endregion

        #region ActivityDTO Response Methods

        public static ActivityResponseDTO AddActivityDTO(this ActivityResponseDTO activityResponse, ActivityDTO activityDTO)
        {
            return AddBaseDTO<ActivityDTO>(activityResponse, ActivityPropertyName, activityDTO);
        }

        public static bool TryParseActivityDTO(this ActivityResponseDTO activityResponse, out ActivityDTO activityDTO)
        {
            return TryParseBaseDTO<ActivityDTO>(activityResponse, ActivityPropertyName, out activityDTO);
        }

        #endregion

        #region DocumentationDTO Response Methods

        public static ActivityResponseDTO AddDocumentationDTO(this ActivityResponseDTO activityResponse, DocumentationDTO documentationDTO)
        {
            return AddBaseDTO<DocumentationDTO>(activityResponse, DocumentationPropertyName, documentationDTO);
        }

        public static bool TryParseDocumentationDTO(this ActivityResponseDTO activityResponse, out DocumentationDTO documentationDTO)
        {
            return TryParseBaseDTO<DocumentationDTO>(activityResponse, DocumentationPropertyName, out documentationDTO);
        }

        #endregion
    }
}
