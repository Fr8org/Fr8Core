using System;
using System.Collections.Generic;
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
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Web.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _action;
        private readonly IActivityTemplate _activityTemplate;

        public ActionController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        public ActionController(IAction service)
        {
            _action = service;
        }


        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [HttpPost]
        [Route("configuration")]
        [Route("configure")]
        //[ResponseType(typeof(CrateStorageDTO))]
        public async Task<IHttpActionResult> Configure(ActionDTO curActionDesignDTO)
        {
            curActionDesignDTO.CurrentView = null;
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            ActionDTO actionDTO = await _action.Configure(curActionDO);
            return Ok(actionDTO);
        }


        [HttpGet]
        [Route("auth_url")]
        public async Task<IHttpActionResult> GetExternalAuthUrl(
            [FromUri(Name = "id")] int activityTemplateId)
        {
            DockyardAccountDO account;
            PluginDO plugin;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(activityTemplateId);

                if (activityTemplate == null)
                {
                    throw new ApplicationException("ActivityTemplate was not found.");
                }

                plugin = activityTemplate.Plugin;

                var accountId = User.Identity.GetUserId();
                account = uow.UserRepository.FindOne(x => x.Id == accountId);

                if (account == null)
                {
                    throw new ApplicationException("User was not found.");
                }
            }

            var externalAuthUrlDTO = await _action.GetExternalAuthUrl(account, plugin);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }

        [HttpPost]
        [Route("authenticate")]
        public async Task<IHttpActionResult> Authenticate(CredentialsDTO credentials)
        {
            DockyardAccountDO account;
            PluginDO plugin;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(credentials.ActivityTemplateId);

                if (activityTemplate == null)
                {
                    throw new ApplicationException("ActivityTemplate was not found.");
                }

                plugin = activityTemplate.Plugin;


                var accountId = User.Identity.GetUserId();
                account = uow.UserRepository.FindOne(x => x.Id == accountId);
                
                if (account == null)
                {
                    throw new ApplicationException("User was not found.");
                }
            }

            await _action.AuthenticateInternal(
                account,
                plugin,
                credentials.Username,
                credentials.Password);

            return Ok();
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public ActionDTO Get(int id)
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
        [Route("{id:int}")]
        public void Delete(int id)
        {
            _action.Delete(id);
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        [Route("save")]
        public IHttpActionResult Save(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var resultActionDO = _action.SaveOrUpdateAction(uow, submittedActionDO);
               
                if (curActionDTO.IsTempId)
                {
                    ObjectFactory.GetInstance<ISubroute>().AddAction(uow, resultActionDO); // append action to the Subroute
                }

                var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

                return Ok(resultActionDTO);
            }
        }    
    }
}