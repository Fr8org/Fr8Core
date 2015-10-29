using System;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;

namespace HubWeb.Controllers
{
    public class DockyardEventController : ApiController
    {
        private readonly IDockyardEvent _dockyardEvent;
        private readonly ICrateManager _crate;
        private IRoute _route;


        public DockyardEventController()
        {
            _dockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _route = ObjectFactory.GetInstance<IRoute>();

        }

        [HttpPost]
        [Route("dockyard_events")]
        public async Task<IHttpActionResult> ProcessDockyardEvents(CrateDTO curCrateStandardEventReport)
        {
            //check if its not null
            if (curCrateStandardEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null.");
            //check if Standard Event Report inside CrateDTO
            if (String.IsNullOrEmpty(curCrateStandardEventReport.ManifestType) || !curCrateStandardEventReport.ManifestType.Equals("Standard Event Report", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentNullException("CrateDTO passed is not a Standard Event Report.");
            if (String.IsNullOrEmpty(curCrateStandardEventReport.Contents))
                throw new ArgumentNullException("CrateDTO Content is empty.");
             
            await _dockyardEvent.ProcessInboundEvents(curCrateStandardEventReport);
           

            return Ok();
        }
    }
}