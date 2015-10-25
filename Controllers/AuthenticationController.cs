﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Infrastructure.StructureMap;

namespace Web.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly ISecurityServices _security;
        private readonly Authorization _authorization;


        public AuthenticationController()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _authorization = new Authorization();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [Route("token")]
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

        [HttpGet]
        [Fr8ApiAuthorize]
        [Route("initial_url")]
        public async Task<IHttpActionResult> GetOAuthInitiationURL(
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

            var externalAuthUrlDTO = await _authorization.GetOAuthInitiationURL(account, activityTemplate);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }
    }
}