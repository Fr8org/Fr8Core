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
}
