using Fr8Data.Constants;
using Newtonsoft.Json;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Control;

namespace Fr8Data.DataTransferObjects
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

        public static ActivityResponseDTO CreateDocumentationResponse(string displayMechanism, string contentPath = "")
        {
            return Create(ActivityResponse.Null)
                .AddDocumentationDTO(new DocumentationDTO(displayMechanism, contentPath));
        }
    }
}
