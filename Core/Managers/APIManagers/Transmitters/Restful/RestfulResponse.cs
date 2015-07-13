using System.Collections.Generic;
using System.Net;

namespace Core.Managers.APIManagers.Transmitters.Restful
{
    /// <summary>
    /// Container class for common properties shared by RestResponse
    /// </summary>
    public class RestfulResponse
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RestfulResponse()
        {
            Headers = new Dictionary<string, string>();
        }
 
        /// <summary>
        /// HTTP response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Description of HTTP status returned
        /// </summary>
        public string StatusDescription { get; set; }
        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Headers returned by server with the response
        /// </summary>
        public Dictionary<string, string> Headers { get; protected internal set; }
    }
}
