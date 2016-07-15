using System;
using System.Net;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

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
        /// <param name="page">Ordinal number of subset of log records to retrieve</param>
        /// <param name="isDescending">Whether to perform sort of results in descending order</param>
        /// <param name="isCurrentUser">Whether to show log records of current user only</param>
        /// <param name="itemPerPage">Max number of log records to retrieve in single response</param>
        /// <param name="filter">Part of textual field of log record to filter by</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ResponseType(typeof(HistoryResultDTO<FactDTO>))]
        [ResponseType(typeof(HistoryResultDTO<IncidentDTO>))]
        [SwaggerResponse(HttpStatusCode.OK, "Collection of log records", typeof(HistoryResultDTO<IncidentDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Incorrect type is specified")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
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