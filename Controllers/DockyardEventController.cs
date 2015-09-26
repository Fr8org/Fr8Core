using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;

namespace Web.Controllers
{
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
        [Route("dockyard_events")]
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

            // Commented out by yakov.gnusin.
            // We cannot use User.Identity.GetUserId() here,
            // since this is asynchronous API call, no authorized user in HttpContext here.
            // _dockyardEvent.ProcessInbound(User.Identity.GetUserId(), eventReportMS);

            // Added by yakov.gnusin, for test purposes only!!! Fix that later.
            // Get first active ProcessTemplate and get its UserID.
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActiveProcessTemplate = uow.ProcessTemplateRepository
                    .GetQuery()
                    .FirstOrDefault(x => x.ProcessTemplateState == ProcessTemplateState.Active);

                if (curActiveProcessTemplate != null)
                {
                    _dockyardEvent.ProcessInbound(curActiveProcessTemplate.DockyardAccount.Id, eventReportMS);
                    
                }
                else
                {
                    _dockyardEvent.ProcessInbound(User.Identity.GetUserId(), eventReportMS);
                }
            }

            return Ok();
        }
    }
}