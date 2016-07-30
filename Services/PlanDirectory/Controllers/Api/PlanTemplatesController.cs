using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Interfaces;
using Microsoft.AspNet.Identity;
using StructureMap;
using Hub.Infrastructure;
using log4net;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Controllers.Api
{
    [RoutePrefix("plan_templates")]
    public class PlanTemplatesController : ApiController
    {
        private readonly IHubCommunicator _hubCommunicator;
        private readonly IPlanTemplate _planTemplate;
        private readonly ISearchProvider _searchProvider;
        private readonly IWebservicesPageGenerator _webservicesPageGenerator;
        private static readonly ILog Logger = LogManager.GetLogger("PlanDirectory");

        public PlanTemplatesController()
        {
            var factory = ObjectFactory.GetInstance<IHubCommunicatorFactory>();
            _hubCommunicator = factory.Create(User.Identity.GetUserId());
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
            _searchProvider = ObjectFactory.GetInstance<ISearchProvider>();
            _webservicesPageGenerator = ObjectFactory.GetInstance<IWebservicesPageGenerator>();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Post(PublishPlanTemplateDTO dto)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateCM = await _planTemplate.CreateOrUpdate(fr8AccountId, dto);
            await _searchProvider.CreateOrUpdate(planTemplateCM);
            await _webservicesPageGenerator.Generate(planTemplateCM, fr8AccountId);
            return Ok();
        }

        [HttpDelete]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            var identity = User.Identity as ClaimsIdentity;
            var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");

            var fr8AccountId = identity.GetUserId();
            var planTemplateCM = await _planTemplate.Get(fr8AccountId, id);

            if (planTemplateCM.OwnerId != fr8AccountId && !privileged)
            {
                return Unauthorized();
            }

            if (planTemplateCM != null)
            {
                await _planTemplate.Remove(fr8AccountId, id);
                await _searchProvider.Remove(id);
            }

            return Ok();
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateDTO = await _planTemplate.GetPlanTemplateDTO(fr8AccountId, id);

            return Ok(planTemplateDTO);
        }

        [HttpGet]
        public async Task<IHttpActionResult> Search(
            string text, int? pageStart = null, int? pageSize = null)
        {
            var searchRequest = new SearchRequestDTO()
            {
                Text = text,
                PageStart = pageStart.GetValueOrDefault(),
                PageSize = pageSize.GetValueOrDefault()
            };

            var searchResult = await _searchProvider.Search(searchRequest);

            return Ok(searchResult);
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> CreatePlan(Guid id)
        {
            try
            {
                var fr8AccountId = User.Identity.GetUserId();
                var planTemplateDTO = await _planTemplate.GetPlanTemplateDTO(fr8AccountId, id);

                if (planTemplateDTO == null)
                {
                    throw new ApplicationException("Unable to find PlanTemplate in MT-database.");
                }

                var plan = await _hubCommunicator.LoadPlan(planTemplateDTO.PlanContents);

                return Ok(
                    new
                    {
                        RedirectUrl = CloudConfigurationManager.GetSetting("HubApiBaseUrl").Replace("/api/v1/", "")
                            + "/dashboard/plans/" + plan.Id.ToString() + "/builder?viewMode=plan"
                    }
                );
            }
            catch (ApplicationException exception)
            {
                Logger.Error(exception.Message);
                return Content(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GeneratePages()
        {
            var searchRequest = new SearchRequestDTO()
            {
                Text = string.Empty,
                PageStart = 0,
                PageSize = 0
            };

            var searchResult = await _searchProvider.Search(searchRequest);

            var fr8AccountId = User.Identity.GetUserId();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var searchItemDto in searchResult.PlanTemplates)
            {
                var planTemplateDto = await _planTemplate.GetPlanTemplateDTO(fr8AccountId, searchItemDto.ParentPlanId);
                if (planTemplateDto == null)
                {
                    continue;
                }
                var planTemplateCm = await _planTemplate.CreateOrUpdate(fr8AccountId, planTemplateDto);
                await _searchProvider.CreateOrUpdate(planTemplateCm);
                await _webservicesPageGenerator.Generate(planTemplateCm, fr8AccountId);
            }
            watch.Stop();
            var elapsed = watch.Elapsed;
            Logger.Info($"Page Generator elapsed time: {elapsed.Minutes} minutes, {elapsed.Seconds} seconds");

            return Ok();
        }
    }
}