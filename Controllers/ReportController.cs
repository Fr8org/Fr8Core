using System;
using System.Collections.Generic;
using System.Web.Http;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using Utilities;
using Utilities.Logging;

namespace HubWeb.Controllers
{    
    [Fr8ApiAuthorize]
    public class ReportController : ApiController
    {
        private IReport _report;        

        public ReportController()
        {
            _report = ObjectFactory.GetInstance<IReport>();
            
        }

        //[Route("api/report/getallfacts")]
        public IHttpActionResult GetAllFacts()
        {
            List<FactDO> factDOList = null;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    factDOList = _report.GetAllFacts(uow);                                    
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(factDOList);
        }

        //[Route("api/report/getallincidents")]
        public IHttpActionResult GetALLIncidents()
        {
            List<IncidentDO> incidentList = null;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    incidentList = _report.GetAllIncidents(uow);                   
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(incidentList);
        }
	}
}