using System;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Web.Controllers
{
    [Authorize]
    [RoutePrefix("dockyard")]
    public class DockyardEventController : ApiController
    {
        private readonly IDockyardEvent _dockyardEvent;
        private readonly ICrate _crate;
        public DockyardEventController()
        {
            _dockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        [HttpPost]
        public IHttpActionResult dockyard_events(CrateDTO curCrateStandardEventReport)
        {
            //check if its not null
            if (curCrateStandardEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null.");
            //check if Standard Event Report inside CrateDTO
            if (String.IsNullOrEmpty(curCrateStandardEventReport.ManifestType) || !curCrateStandardEventReport.ManifestType.Equals("Standard Event Report", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentNullException("CrateDTO passed is not a Standard Event Report.");
            if (String.IsNullOrEmpty(curCrateStandardEventReport.Contents))
                throw new ArgumentNullException("CrateDTO Content is empty.");

            EventReportMS eventReportMS = _crate.GetContents<EventReportMS>(curCrateStandardEventReport);

            //call DockyardEvent#ProcessInbound
            _dockyardEvent.ProcessInbound(User.Identity.GetUserId(), eventReportMS);

            return Ok();
        }
    }
}