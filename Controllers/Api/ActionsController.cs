using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Hub.Services;
using HubWeb.Controllers.Helpers;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Infrastructure;

namespace HubWeb.Controllers
{
    
    [Fr8HubWebHMACAuthorize]
    [Fr8ApiAuthorize]
    public class ActionsController : ApiController
    {
        private readonly IAction _action;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ISubroute _subRoute;
        private readonly IRoute _route;

        private readonly IAuthorization _authorization;

        public ActionsController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _subRoute = ObjectFactory.GetInstance<ISubroute>();
            _route = ObjectFactory.GetInstance<IRoute>();
            _authorization = ObjectFactory.GetInstance<IAuthorization>();
        }

        public ActionsController(IAction service)
        {
            _action = service;
        }

        public ActionsController(ISubroute service)
        {
            _subRoute = service;
        }


        [HttpPost]
        public async Task<IHttpActionResult> Create(int actionTemplateId, string name, string label = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userId = User.Identity.GetUserId();

                var result = await _action.CreateAndConfigure(uow, userId, actionTemplateId, name, label, parentNodeId, createRoute, authorizationTokenId);

                if (result is ActionDO)
                {
                    return Ok(Mapper.Map<ActionDTO>(result));
                }

                if (result is RouteDO)
                {
                    return Ok(RouteMappingHelper.MapRouteToDto(uow, (RouteDO)result));
                }

                throw new Exception("Unsupported type " + result.GetType());
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Create(string solutionName)
        {
            var userId = User.Identity.GetUserId();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetAll()
                    .FirstOrDefault(at => at.Name == solutionName);
                if (activityTemplate == null)
                {
                    throw new ArgumentException(String.Format("actionTemplate (solution) name {0} is not found in the database.", solutionName));
                }

                var result = await _action.CreateAndConfigure(uow, userId,
                    activityTemplate.Id, activityTemplate.Name, activityTemplate.Label, null, true);
                return Ok(RouteMappingHelper.MapRouteToDto(uow, (RouteDO)result));
            }
        }


        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [HttpPost]
        public async Task<IHttpActionResult> Configure(ActionDTO curActionDesignDTO)
        {
            // WebMonitor.Tracer.Monitor.StartMonitoring("Configuring action " + curActionDesignDTO.Name);
            curActionDesignDTO.CurrentView = null;
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            ActionDTO actionDTO = await _action.Configure(User.Identity.GetUserId(), curActionDO);
            return Ok(actionDTO);
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActionDTO Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return Mapper.Map<ActionDTO>(_action.GetById(uow, id));
            }
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpDelete]
        //[Route("{id:guid}")]
        public async Task<IHttpActionResult> Delete(Guid id, bool confirmed = false)
        {
            var isDeleted = await _subRoute.DeleteAction(User.Identity.GetUserId(), id, confirmed);
            if (!isDeleted)
            {
                return ResponseMessage(new HttpResponseMessage(System.Net.HttpStatusCode.PreconditionFailed));
            }
            return Ok();
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IHttpActionResult Save(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var resultActionDO = _action.SaveOrUpdateAction(uow, submittedActionDO);
                var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

                return Ok(resultActionDTO);
            }
        }

//        /// <summary>
//        /// POST : updates the given action
//        /// </summary>
//        [HttpPost]
//        [Route("update")]
//        public IHttpActionResult Update(ActionDTO curActionDTO)
//        {
//            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);
//
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                await _action.SaveUpdateAndConfigure(uow, submittedActionDO);
//            }
//
//            return Ok();
//        }    
            }
}