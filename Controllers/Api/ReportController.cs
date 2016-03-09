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
using Data.Interfaces.DataTransferObjects;
using System.Linq;
using AutoMapper;

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
            IEnumerable<FactDTO> factDTOList = null;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    factDTOList = _report.GetAllFacts(uow).Select(f => Mapper.Map<FactDTO>(f));
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(factDTOList);
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
        //[Route("api/report/getincidents?page={page}&pageSize={pageSize}")]
        public IHttpActionResult GetIncidents(int page, int pageSize)
        {
            List<IncidentDO> incidentList = null;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    incidentList = _report.GetIncidents(uow, page, pageSize);
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