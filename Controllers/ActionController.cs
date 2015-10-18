using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
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

namespace Web.Controllers
{
    [System.Web.Http.RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private readonly IAction _action;
        private readonly IRoute _route;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly ISubroute _subRoute;

        public ActionController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _route = ObjectFactory.GetInstance<IRoute>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _subRoute = ObjectFactory.GetInstance<ISubroute>();
        }

        public ActionController(IAction service)
        {
            _action = service;
        }

        public ActionController(ISubroute service)
        {
            _subRoute = service;
        }


        [System.Web.Http.HttpGet]
        [Fr8ApiAuthorize]
        [System.Web.Http.Route("create")]
        public async Task<IHttpActionResult> Create(int actionTemplateId, string name, string label = null, int? parentNodeId = null, bool createRoute = false)
        {
            if (parentNodeId != null && createRoute)
            {
                throw new ArgumentException("Parent node id can't be set together with create route flag");
            }

            if (parentNodeId == null && !createRoute)
            {
                throw new ArgumentException("Either Parent node id or create route flag must be set");
            }
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RouteNodeDO parentNode;
                RouteDO route = null;

                if (createRoute)
                {
                    SubrouteDO subroute;
                    
                    route = _route.CreateRouteWithOneSubroute(uow, name, out subroute);
                    parentNode = subroute;
                }
                else
                {
                    parentNode = uow.RouteNodeRepository.GetByKey(parentNodeId.Value);
                }
                
                var action = _action.Create(uow, actionTemplateId, name, label, parentNode);
                
                uow.SaveChanges();

                await _action.Configure(action);

                if (createRoute)
                {
                    return Ok(_route.MapRouteToDto(uow, route));
                }

                return Ok(Mapper.Map<ActionDTO>(action));
            }
        }
        
        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("configuration")]
        [System.Web.Http.Route("configure")]
        //[ResponseType(typeof(CrateStorageDTO))]
        public async Task<IHttpActionResult> Configure(ActionDTO curActionDesignDTO)
        {
            curActionDesignDTO.CurrentView = null;
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDesignDTO);
            ActionDTO actionDTO = await _action.Configure(curActionDO);
            return Ok(actionDTO);
        }


        [System.Web.Http.HttpGet]
        [Fr8ApiAuthorize]
        [System.Web.Http.Route("auth_url")]
        public async Task<IHttpActionResult> GetExternalAuthUrl(
            [FromUri(Name = "id")] int activityTemplateId)
        {
            Fr8AccountDO account;
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

                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _action.GetExternalAuthUrl(account, plugin);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }

        private void ExtractPluginAndAccount(int activityTemplateId,
           out Fr8AccountDO account, out PluginDO plugin)
        {
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
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("is_authenticated")]
        public IHttpActionResult IsAuthenticated(int activityTemplateId)
        {
            Fr8AccountDO account;
            PluginDO plugin;

            ExtractPluginAndAccount(activityTemplateId, out account, out plugin);

            var isAuthenticated = _action.IsAuthenticated(account, plugin);

            return Ok(new { authenticated = isAuthenticated });
        }

        [System.Web.Http.HttpPost]
        [Fr8ApiAuthorize]
        [System.Web.Http.Route("authenticate")]
        public async Task<IHttpActionResult> Authenticate(CredentialsDTO credentials)
        {
            Fr8AccountDO account;
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
                account = _security.GetCurrentAccount(uow);
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
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("{id:int}")]
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
        [System.Web.Http.HttpDelete]
        [System.Web.Http.Route("{id:int}")]
        public void Delete(int id)
        {
            _subRoute.DeleteAction(id);
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("save")]
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

        /// <summary>
        /// POST : updates the given action
        /// </summary>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("update")]
        public IHttpActionResult Update(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _action.Update(uow, submittedActionDO);
            }

            return Ok();
        }    
    }
}