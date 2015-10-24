using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;

namespace Web.Controllers
{
    [RoutePrefix("solutions")]
    public class SolutionController : ApiController
    {
        private readonly IAction _action;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ISubroute _subRoute;
        private readonly IRoute _route;

        private readonly Authorization _authorization;

        public SolutionController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _subRoute = ObjectFactory.GetInstance<ISubroute>();
            _route = ObjectFactory.GetInstance<IRoute>();
            _authorization = new Authorization();
        }

        public SolutionController(IAction service)
        {
            _action = service;
        }

        public SolutionController(ISubroute service)
        {
            _subRoute = service;
        }


        [Fr8ApiAuthorize]
        [Route("create")]
        public async Task<IHttpActionResult> Post(string solutionName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int actionTemplateId = uow.ActivityTemplateRepository.GetAll().
                    Where(at => at.Name == solutionName).Select(at => at.Id).FirstOrDefault();
                if (actionTemplateId == 0)
                {
                    throw new ArgumentException(String.Format("actionTemplate (solution) name {0} is not found in the database.", solutionName));
                }

                var result = await _action.CreateAndConfigure(uow, actionTemplateId, "Solution", null, null, true);
                return Ok(_route.MapRouteToDto(uow, (RouteDO)result));
            }
        }
    }
}