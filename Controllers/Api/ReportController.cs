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
        private readonly IReport _report;

        public ReportController()
        {
            _report = ObjectFactory.GetInstance<IReport>();
        }

        [Fr8ApiAuthorize]
        [ActionName("getIncidentsByQuery")]
        [HttpGet]
        public IHttpActionResult GetIncidentsByQuery([FromUri] HistoryQueryDTO historyQueryDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = _report.GetIncidents(uow, historyQueryDTO);
                return Ok(result);
            }
        }

        [Fr8ApiAuthorize]
        [ActionName("getFactsByQuery")]
        [HttpGet]
        public IHttpActionResult GetFactsByQuery([FromUri] HistoryQueryDTO historyQueryDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = _report.GetFacts(uow, historyQueryDTO);
                return Ok(result);
            }
        }
    }
}