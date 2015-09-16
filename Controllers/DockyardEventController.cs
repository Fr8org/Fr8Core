using System;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Microsoft.AspNet.Identity;

namespace Web.Controllers
{
    [Authorize]
    [RoutePrefix("dockyard_events")]
    public class DockyardEventController : ApiController
    {
        private readonly IDockyardEvent _dockyardEvent;

        public DockyardEventController()
        {
            _dockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();
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

            //call DockyardEvent#ProcessInbound
            _dockyardEvent.ProcessInbound(User.Identity.GetUserId(), curCrateStandardEventReport);

            return Ok();
        }
    }
}