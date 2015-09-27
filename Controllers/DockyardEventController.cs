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
        private IProcessTemplate _processTemplate;


        public DockyardEventController()
        {
            _dockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();
            _crate = ObjectFactory.GetInstance<ICrate>();
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();

        }

        [HttpPost]
        [Route("dockyard_events")]
        public IHttpActionResult ProcessDockyardEvents(CrateDTO curCrateStandardEventReport)
        {
            //check if its not null
            if (curCrateStandardEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null.");
            //check if Standard Event Report inside CrateDTO
            if (String.IsNullOrEmpty(curCrateStandardEventReport.ManifestType) || !curCrateStandardEventReport.ManifestType.Equals("Standard Event Report", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentNullException("CrateDTO passed is not a Standard Event Report.");
            if (String.IsNullOrEmpty(curCrateStandardEventReport.Contents))
                throw new ArgumentNullException("CrateDTO Content is empty.");
            _dockyardEvent.ProcessInboundEvents(curCrateStandardEventReport);
           

            return Ok();
        }
    }
}