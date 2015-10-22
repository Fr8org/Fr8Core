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
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _action;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ICrateManager _crate;
        private readonly ISubroute _subRoute;
        private readonly Authorization _authorization;

        public ActionController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _subRoute = ObjectFactory.GetInstance<ISubroute>();
            _authorization = new Authorization();
        }

        public ActionController(IAction service)
        {
            _action = service;
        }

        public ActionController(ISubroute service)
        {
            _subRoute = service;
        }

        private void AddAuthenticationCrate(
            ActionDTO actionDTO, int authType)
        {
            if (actionDTO.CrateStorage == null)
            {
                actionDTO.CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new List<CrateDTO>()
                };
            }

            var mode = authType == AuthenticationType.Internal
                ? AuthenticationMode.InternalMode
                : AuthenticationMode.ExternalMode;

            actionDTO.CrateStorage.CrateDTO.Add(
                _crate.CreateAuthenticationCrate("RequiresAuthentication", mode)
            );
        }

        protected void RemoveAuthenticationCrate(ActionDTO actionDTO)
        {
            if (actionDTO.CrateStorage != null
                && actionDTO.CrateStorage.CrateDTO != null)
            {
                var authCrates = actionDTO.CrateStorage.CrateDTO
                    .Where(x => x.ManifestType == CrateManifests.STANDARD_AUTHENTICATION_NAME)
                    .ToList();

                foreach (var authCrate in authCrates)
                {
                    actionDTO.CrateStorage.CrateDTO.Remove(authCrate);
                }
            }
        }

        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [HttpPost]
        [Route("configuration")]
        [Route("configure")]
        //[ResponseType(typeof(CrateStorageDTO))]
        public async Task<IHttpActionResult> Configure(ActionDTO curActionDesignDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(curActionDesignDTO.ActivityTemplateId);

                if (activityTemplate == null)
                {
                    throw new ApplicationException("ActivityTemplate was not found.");
                }

                var account = uow.UserRepository.GetByKey(User.Identity.GetUserId());

                if (account == null)
                {
                    throw new ApplicationException("Current account was not found.");
                }

                if (activityTemplate.AuthenticationType != AuthenticationType.None)
                {
                    var authToken = uow.AuthorizationTokenRepository
                        .FindOne(x => x.Plugin.Id == activityTemplate.Plugin.Id
                            && x.UserDO.Id == account.Id);

                    if (authToken == null)
                    {
                        AddAuthenticationCrate(curActionDesignDTO, activityTemplate.AuthenticationType);
                        return Ok(curActionDesignDTO);
                    }
                    else
                    {
                        RemoveAuthenticationCrate(curActionDesignDTO);
                    }
                }
            }

            curActionDesignDTO.CurrentView = null;
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            ActionDTO actionDTO = await _action.Configure(curActionDO);
            return Ok(actionDTO);
        }


        [HttpGet]
        [Fr8ApiAuthorize]
        [Route("auth_url")]
        public async Task<IHttpActionResult> GetExternalAuthUrl(
            [FromUri(Name = "id")] int activityTemplateId)
        {
            Fr8AccountDO account;
            ActivityTemplateDO activityTemplate;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityTemplate = uow.ActivityTemplateRepository
                    .GetQuery()
                    .Include(x => x.Plugin)
                    .SingleOrDefault(x => x.Id == activityTemplateId);

                if (activityTemplate == null)
                {
                    throw new ApplicationException("ActivityTemplate was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _authorization.GetExternalAuthUrl(account, activityTemplate);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [Route("authenticate")]
        public async Task<IHttpActionResult> Authenticate(CredentialsDTO credentials)
        {
            Fr8AccountDO account;
            ActivityTemplateDO activityTemplate;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityTemplate = uow.ActivityTemplateRepository
                    .GetQuery()
                    .Include(x => x.Plugin)
                    .SingleOrDefault(x => x.Id == credentials.ActivityTemplateId);

                if (activityTemplate == null)
                {
                    throw new ApplicationException("ActivityTemplate was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            await _authorization.AuthenticateInternal(
                account,
                activityTemplate,
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
            _subRoute.DeleteAction(id);
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