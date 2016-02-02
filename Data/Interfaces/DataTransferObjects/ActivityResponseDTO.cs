using System.Web.Caching;
using Data.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// Data structure intended to enable Fr8 services to return useful data as part of the http response
    /// </summary>
    public class ActivityResponseDTO 
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        public static ActivityResponseDTO Create(ActivityResponse activityResponseType)
        {
            return new ActivityResponseDTO()
            {
                Type = activityResponseType.ToString()
            };
        }
    }

    public static class ActivityResponseHelper
    {
        public static ActivityResponseDTO AddErrorDTO(this ActivityResponseDTO activityResponse, ErrorDTO errorDTO)
        {
            JToken token = JsonConvert.SerializeObject(errorDTO);
            JObject responseBody = JObject.Parse(activityResponse.Body);

            responseBody.Add("error", token);
            activityResponse.Body = JsonConvert.SerializeObject(responseBody);

            return activityResponse;
        }

        public static void TryParseErrorDTO(this ActivityResponseDTO activityResponse, out ErrorDTO errorDTO)
        {
            errorDTO = null;
            JObject responseBody = JObject.Parse(activityResponse.Body);
            var errorToken = responseBody["error"];
            if (errorToken != null)
            {
                errorDTO = JsonConvert.DeserializeObject<ErrorDTO>(errorToken.ToString());
            }
        }

        public static ActivityResponseDTO AddPayloadDTO(this ActivityResponseDTO activityResponse, PayloadDTO payloadDTO)
        {
            JObject responseBody = JObject.Parse(activityResponse.Body);
            JToken token = JsonConvert.SerializeObject(payloadDTO);
            //check if already exists
            responseBody.Add("payload", token);

            return activityResponse;
        }

        public static void TryParsePayloadDTO(this ActivityResponseDTO activityResponse, out PayloadDTO payloadDTO)
        {
            payloadDTO = null;
            JObject responseBody = JObject.Parse(activityResponse.Body);

            var payloadToken = responseBody["payload"];
            if (payloadToken != null)
            {
                payloadDTO = JsonConvert.DeserializeObject<PayloadDTO>(payloadToken.ToString());
            }
        }

        public static ActivityResponseDTO AddResponseMessageDTO(this ActivityResponseDTO activityResponse, ResponseMessageDTO responseMessageDTO)
        {
            JToken token = JsonConvert.SerializeObject(responseMessageDTO);
            //check if already exists
            activityResponse.Body.Add("responseMessage", token);

            return activityResponse;
        }

        public static void TryParseResponseMessageDTO(this ActivityResponseDTO activityResponse, out ResponseMessageDTO responseMessageDTO)
        {
            //...   
        }

        public static ActivityResponseDTO AddActivityDTO(this ActivityResponseDTO activityResponse, ActivityDTO activityDTO)
        {
            JToken token = JsonConvert.SerializeObject(activityDTO);
            //check if already exists
            activityResponse.Body.Add("activity", token);

            return activityResponse;
        }

        public static void TryParseActivityDTO(this ActivityResponseDTO activityResponse, out ActivityDTO activityDTO)
        {
        }
    }
}
