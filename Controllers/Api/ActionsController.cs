using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
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
    [Fr8ApiAuthorize]
    public class ActionsController : ApiController
    {
        private readonly IActivity _activity;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ISubroute _subRoute;
        private readonly Hub.Interfaces.IPlan _plan;

        private readonly IAuthorization _authorization;

        public ActionsController()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _subRoute = ObjectFactory.GetInstance<ISubroute>();
            _plan = ObjectFactory.GetInstance<IPlan>();
            _authorization = ObjectFactory.GetInstance<IAuthorization>();
        }

        public ActionsController(IActivity service)
        {
            _activity = service;
        }

        public ActionsController(ISubroute service)
        {
            _subRoute = service;
        }


        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Create(int actionTemplateId, string name, string label = null, int? order = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userId = User.Identity.GetUserId();

                var result = await _activity.CreateAndConfigure(uow, userId, actionTemplateId, name, label, order, parentNodeId, createRoute, authorizationTokenId);

                if (result is ActivityDO)
                {
                    return Ok(Mapper.Map<ActivityDTO>(result));
                }

                if (result is PlanDO)
                {
                    return Ok(RouteMappingHelper.MapRouteToDto(uow, (PlanDO)result));
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
                var activityTemplate = _activityTemplate.GetQuery().FirstOrDefault(at => at.Name == solutionName);

                if (activityTemplate == null)
                {
                    throw new ArgumentException(String.Format("actionTemplate (solution) name {0} is not found in the database.", solutionName));
                }

                var result = await _activity.CreateAndConfigure(uow, userId,
                    activityTemplate.Id, activityTemplate.Name, activityTemplate.Label, null, null, true);
                return Ok(RouteMappingHelper.MapRouteToDto(uow, (PlanDO)result));
            }
        }


        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO)
        {
            // WebMonitor.Tracer.Monitor.StartMonitoring("Configuring action " + curActionDesignDTO.Name);
            curActionDesignDTO.CurrentView = null;
            ActivityDO curActivityDO = Mapper.Map<ActivityDO>(curActionDesignDTO);
            ActivityDTO activityDTO = await _activity.Configure(User.Identity.GetUserId(), curActivityDO);
            return Ok(activityDTO);
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActivityDTO Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return Mapper.Map<ActivityDTO>(_activity.GetById(uow, id));
            }
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpDelete]
        //[Route("{id:guid}")]
        public async Task<IHttpActionResult> Delete(Guid id, bool confirmed = false)
        {
            var isDeleted = await _subRoute.DeleteActivity(User.Identity.GetUserId(), id, confirmed);
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
        public IHttpActionResult Save(ActivityDTO curActionDTO)
        {
            ActivityDO submittedActivityDO = Mapper.Map<ActivityDO>(curActionDTO);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var resultActionDO = _activity.SaveOrUpdateActivity(uow, submittedActivityDO);
                var resultActionDTO = Mapper.Map<ActivityDTO>(resultActionDO);
                return Ok(resultActionDTO);
            }
        }
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Documentation(ActivityDTO curActivityDTO)
        {
            var curDocSupport = curActivityDTO.DocumentationSupport;
            if (!ValidateDocumentationSupport(curDocSupport))
                return BadRequest();
            var solutionPageDTO = await _activity.GetSolutionDocumentation(curActivityDTO);
            return Ok(solutionPageDTO);

        }

        private bool ValidateDocumentationSupport(string docSupport)
        {
            var curStringArray = docSupport.Split(',');
            if (curStringArray.Contains("MainPage") && curStringArray.Contains("HelpMenu"))
                throw new Exception("ActionDTO cannot have both MainPage and HelpMenu in the Documentation Support field value");
            if (curStringArray.Contains("MainPage") || curStringArray.Contains("HelpMenu"))
                return true;
            return false;
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