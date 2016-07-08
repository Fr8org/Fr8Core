﻿using System.Web.Http;
using System.Web.Http.Description;
using WebApi.OutputCache.V2;

namespace HubWeb.Controllers.Api
{
    public class ConfigurationController : ApiController
    {
        /// <summary>
        /// Returns instrumentation key for the telemetry service 
        /// </summary>
        /// <response code="200">String containing instrumentation key</response>
        [ActionName("instrumentation-key"), CacheOutput(ServerTimeSpan = 600, ClientTimeSpan = 600, ExcludeQueryStringFromCacheKey = true)]
        [ResponseType(typeof(string))]
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