using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;
using WebApi.OutputCache.V2;

namespace HubWeb.Controllers.Api
{
    public class ConfigurationController : ApiController
    {
        /// <summary>
        /// Returns instrumentation key for the telemetry service 
        /// </summary>
        [ActionName("instrumentation-key"), CacheOutput(ServerTimeSpan = 600, ClientTimeSpan = 600, ExcludeQueryStringFromCacheKey = true)]
        [SwaggerResponse(HttpStatusCode.OK, "String containing instrumentation key", typeof(string))]
        public IHttpActionResult GetTelemetryInstrumentationKey()
        {
            string fileName = "~/ApplicationInsights.config";
            var configDoc = new System.Xml.XmlDocument();
            configDoc.Load(System.Web.Hosting.HostingEnvironment.MapPath(fileName));
            var instrKey = configDoc.GetElementsByTagName("InstrumentationKey")[0].InnerText;
            return Ok(instrKey);
        }
    }
}