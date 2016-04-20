using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Controllers.Helpers;
using HubWeb.Infrastructure;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActivitiesController : ApiController
    {
        private readonly IActivity _activity;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ITerminal _terminal;
        private readonly ISubPlan _subPlan;

        public ActivitiesController()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _subPlan = ObjectFactory.GetInstance<ISubPlan>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        public ActivitiesController(IActivity service)
        {
            _activity = service;
        }

        public ActivitiesController(ISubPlan service)
        {
            _subPlan = service;
        }


        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Create(Guid actionTemplateId, string label = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userId = User.Identity.GetUserId();

                var result = await _activity.CreateAndConfigure(uow, userId, actionTemplateId, label, order, parentNodeId, createPlan, authorizationTokenId);

                if (result is ActivityDO)
                {
                    return Ok(Mapper.Map<ActivityDTO>(result));
                }

                if (result is PlanDO)
                {
                    return Ok(PlanMappingHelper.MapPlanToDto(uow, (PlanDO)result));
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
                    activityTemplate.Id, activityTemplate.Label, null, null, true, null);
                return Ok(PlanMappingHelper.MapPlanToDto(uow, (PlanDO)result));
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

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityDTO activityDTO = await _activity.Configure(uow, User.Identity.GetUserId(), curActivityDO);
                return Ok(activityDTO);
            }
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
            var isDeleted = await _subPlan.DeleteActivity(User.Identity.GetUserId(), id, confirmed);
            if (!isDeleted)
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            return Ok();
        }

        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> DeleteActivity(Guid id)
        {
            await _subPlan.DeleteActivity(User.Identity.GetUserId(), id, true);
            return Ok();
        }

        /// <summary>
        /// DELETE: Remove all child Nodes and clear activity values
        /// </summary>
        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> DeleteChildNodes(Guid activityId)
        {
            var isDeleted = await _subPlan.DeleteAllChildNodes(activityId);
            if (!isDeleted)
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            return Ok();
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO)
        {
            ActivityDO submittedActivityDO = Mapper.Map<ActivityDO>(curActionDTO);

            var resultActionDTO = await _activity.SaveOrUpdateActivity(submittedActivityDO);

            return Ok(resultActionDTO);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Documentation([FromBody] ActivityDTO curActivityDTO)
        {
            var curDocSupport = curActivityDTO.Documentation;
            //check if the DocumentationSupport comma separated string has the correct form
            if (!ValidateDocumentationSupport(curDocSupport))
                return BadRequest();
            if (curDocSupport.StartsWith("Terminal="))
            {
                var terminalName = curDocSupport.Split('=')[1];
                var solutionPages = await _terminal.GetSolutionDocumentations(terminalName);
                return Ok(solutionPages);
            }
            if (curDocSupport.Contains("MainPage"))
            {
                var solutionPageDTO = await _activity.GetActivityDocumentation<SolutionPageDTO>(curActivityDTO, true);
                return Ok(solutionPageDTO);
            }
            if (curDocSupport.Contains("HelpMenu"))
            {
                var activityRepsonceDTO = await _activity.GetActivityDocumentation<ActivityResponseDTO>(curActivityDTO);
                return Ok(activityRepsonceDTO);
            }
            return BadRequest();
        }
        /// <summary>
        /// We currently provide only one substring value, namely 'Terminal=','MainPage' and 'HelpMenu'
        /// </summary>
        /// <param name="docSupport"></param>
        /// <returns></returns>
        private bool ValidateDocumentationSupport(string docSupport)
        {
            var curStringArray = docSupport.Replace(" ", "").Split(',');
            var containsOneSubstring = curStringArray.Count() == 1;
            var hasTerminalName = curStringArray.Any(x => x.StartsWith("Terminal="));
            var hasMainPage = curStringArray.Contains("MainPage");
            var hasHelpMenu = curStringArray.Contains("HelpMenu");
            if ((containsOneSubstring && hasTerminalName) 
                || (containsOneSubstring && hasMainPage) 
                || (containsOneSubstring && hasHelpMenu))
                return true;
            else
                throw new Exception("Incorrect documentation support values");
        }
    }
}