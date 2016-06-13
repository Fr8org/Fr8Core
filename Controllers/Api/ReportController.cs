using System;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;

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

        // TODO: prev endpoints: /getIncidentsByQuery and /getFactsByQuery
        [Fr8ApiAuthorize]
        [HttpGet]
        public IHttpActionResult Get([FromUri] string type, [FromUri] HistoryQueryDTO historyQueryDTO)
        {
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
                    throw new NotSupportedException("Specified report type is not supported");
                }
            }
        }
    }
}