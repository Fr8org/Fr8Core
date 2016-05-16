using System;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActivitiesController : ApiController
    {
        private readonly IActivity _activity;
        private readonly ITerminal _terminal;

        public ActivitiesController()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        public ActivitiesController(IActivity service)
        {
            _activity = service;
        }


        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Create(Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, Guid? authorizationTokenId = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userId = User.Identity.GetUserId();
                var result = await _activity.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, false, authorizationTokenId) as ActivityDO;
                    return Ok(Mapper.Map<ActivityDTO>(result));
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

        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id, bool confirmed = false)
        {
            await _activity.Delete(id);
            return Ok();
        }

        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> DeleteActivity(Guid id)
        {
            await _activity.Delete(id);
            return Ok();
        }

        /// <summary>
        /// DELETE: Remove all child Nodes and clear activity values
        /// </summary>
        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> DeleteChildNodes(Guid activityId)
        {
            await _activity.Delete(activityId);
            return Ok();
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO)
        {
            var submittedActivityDO = Mapper.Map<ActivityDO>(curActionDTO);

            var resultActionDTO = await _activity.SaveOrUpdateActivity(submittedActivityDO);

            return Ok(resultActionDTO);
        }
    }
}