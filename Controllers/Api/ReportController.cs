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

        public IHttpActionResult GetAllFacts()
        {
            IEnumerable<HistoryItemDTO> factDTOList = null;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    factDTOList = _report.GetAllFacts(uow).Select(f => Mapper.Map<HistoryItemDTO>(f));
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(factDTOList);
        }

        [Fr8ApiAuthorize]
        [ActionName("getByQuery")]
        [HttpGet]
        public IHttpActionResult GetByQuery([FromUri] HistoryQueryDTO historyQueryDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = _report.GetIncidents(uow, historyQueryDTO);
                return Ok(result);
            }
        }

        public IHttpActionResult GetIncidents(int page, int pageSize, string user)
        {
            //this is a flag to return either all or user-specific incidents
            bool isCurrentUser;
            var historyItems = new List<HistoryItemDTO>();
            //based on the parameter in GET request decide the value of the flag
            if (string.Equals(user, "current"))
                isCurrentUser = true;
            else if (string.Equals(user, "all"))
                isCurrentUser = false;
            else
                throw new ArgumentException("Incorect user parameter. Only 'current' and 'all' are allowed");
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var incidents = _report.GetIncidents(uow, page, pageSize, isCurrentUser);
                    historyItems.AddRange(incidents.Select(Mapper.Map<HistoryItemDTO>));
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error checking for activity template ", e);
            }
            return Ok(historyItems);
        }
    }
}