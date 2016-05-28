using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using WebApi.OutputCache.V2;

namespace HubWeb.Controllers.Api
{
    public class ConfigurationController : ApiController
    {
        /// <summary>
        /// Returns instrumentation key for the telemetry service 
        /// </summary>
        /// <returns></returns>
        [ActionName("instrumentation-key"), CacheOutput(ServerTimeSpan = 600, ClientTimeSpan = 600, ExcludeQueryStringFromCacheKey = true)]
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