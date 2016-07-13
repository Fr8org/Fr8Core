using System;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Web.Http.Description;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ReportController : ApiController
    {
        private readonly IReport _report;

        public ReportController()
        {
            _report = ObjectFactory.GetInstance<IReport>();
        }
        /// <summary>
        /// Retrieves collection of log records based on query parameters specified
        /// </summary>
        /// <param name="type">Type of log records to return. Supports values of 'incidents' and 'facts'</param>
        /// <param name="historyQueryDTO">Query filter</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <response code="200">Collection of log records based on query filter</response>
        /// <response code="400">Incorrect type is specified</response>
        /// <response code="403">Unauthorized request</response>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ResponseType(typeof(HistoryResultDTO<FactDTO>))]
        [ResponseType(typeof(HistoryResultDTO<IncidentDTO>))]
        public IHttpActionResult Get([FromUri] string type, [FromUri] HistoryQueryDTO historyQueryDTO)
        {
            type = (type ?? string.Empty).Trim().ToLower();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (type == "incidents")
                {
                    var result = _report.GetIncidents(uow, historyQueryDTO);
                    return Ok(result);
                }
                else if (type == "facts")
                {
                    var result = _report.GetFacts(uow, historyQueryDTO);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Specified report type is not supported");
                }
            }
        }
    }
}