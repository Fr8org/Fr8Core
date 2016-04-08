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
using Microsoft.Ajax.Utilities;
using InternalInterface = Hub.Interfaces;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ReportController : ApiController
    {
        private InternalInterface.IReport _report;

        public ReportController()
        {
            _report = ObjectFactory.GetInstance<InternalInterface.IReport>();
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

        //[Route("api/report/GetTopIncidents?page={page}&pageSize={pageSize}&user={current/all}&numOfIncidetns={1000}")]
        public IHttpActionResult GetTopIncidents(int page, int pageSize, string user, int numOfIncidents = 1000)
        {
            //this is a flag to return either all or user-specific incidents
            bool getCurrentUserIncidents;
            var incidentDTOList = new List<IncidentDTO>();
            //based on the parameter in GET request decide the value of the flag
            if (string.Equals(user, "current"))
                getCurrentUserIncidents = true;
            else if (string.Equals(user, "all"))
                getCurrentUserIncidents = false;
            else
            {
                throw new ArgumentException("Incorect user parameter. Only 'current' and 'all' are allowed");
            }
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var incidentDOs = _report.GetTopIncidents(uow, page, pageSize, getCurrentUserIncidents,
                        numOfIncidents);
                    //We map DO->DTO to avoid lazy load entity references that may lead to crash
                    incidentDTOList.AddRange(incidentDOs.Select(incidentDO => Mapper.Map<IncidentDTO>(incidentDO)));
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(incidentDTOList);
        }
    }
}