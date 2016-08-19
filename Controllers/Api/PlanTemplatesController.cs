using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using log4net;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace HubWeb.Controllers.Api
{
    [RoutePrefix("plan_templates")]
    public class PlanTemplatesController : ApiController
    {
        //private readonly IHubCommunicator _hubCommunicator;
        private readonly IPlanTemplate _planTemplate;
        private readonly ISearchProvider _searchProvider;
        private readonly IWebservicesPageGenerator _webservicesPageGenerator;
        private readonly IPlanDirectoryService _planDirectoryService;
        private readonly IPlanTemplateDetailsGenerator _planTemplateDetailsGenerator;
        private static readonly ILog Logger = LogManager.GetLogger("PlanDirectory");

        public PlanTemplatesController()
        {
            //var factory = ObjectFactory.GetInstance<IHubCommunicatorFactory>();
            //_hubCommunicator = factory.Create(User.Identity.GetUserId());
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
            _searchProvider = ObjectFactory.GetInstance<ISearchProvider>();
            _webservicesPageGenerator = ObjectFactory.GetInstance<IWebservicesPageGenerator>();
            _planTemplateDetailsGenerator = ObjectFactory.GetInstance<IPlanTemplateDetailsGenerator>();
            _planDirectoryService = ObjectFactory.GetInstance<IPlanDirectoryService>();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(PublishPlanTemplateDTO dto)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateCM = await _planTemplate.CreateOrUpdate(fr8AccountId, dto);
            await _searchProvider.CreateOrUpdate(planTemplateCM);
            await _webservicesPageGenerator.Generate(planTemplateCM, fr8AccountId);
            await _planTemplateDetailsGenerator.Generate(dto);
            return Ok();
        }

        [HttpDelete]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            var identity = User.Identity as ClaimsIdentity;
            var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");

            var fr8AccountId = identity.GetUserId();
            var planTemplateCM = await _planTemplate.Get(fr8AccountId, id);

            if (planTemplateCM != null)
            {
                if (planTemplateCM.OwnerId != fr8AccountId && !privileged)
                {
                    return Unauthorized();
                }
                await _planTemplate.Remove(fr8AccountId, id);
            }
            //if planTemplate is not in MT we should delete it from azure search
            await _searchProvider.Remove(id);

            return Ok();
        }

        [HttpGet]
        [Fr8ApiAuthorize]
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

                var plan = await _planDirectoryService.CreateFromTemplate(planTemplateDTO.PlanContents, User.Identity.GetUserId());

                return Ok(
                    new
                    {
                        RedirectUrl = CloudConfigurationManager.GetSetting("HubApiUrl").Replace("/api/v1/", "")
                            + "/dashboard/plans/" + plan.Id + "/builder?viewMode=plan"
                    }
                );
            }
            catch (ApplicationException exception)
            {
                Logger.Error(exception.Message);
                return Content(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        [ActionName("details_page")]
        public async Task<IHttpActionResult> DetailsPage(Guid id)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateDTO = await _planTemplate.GetPlanTemplateDTO(fr8AccountId, id);
            if (planTemplateDTO == null)
            {
                return Ok();
            }

            if (!await _planTemplateDetailsGenerator.HasGeneratedPage(planTemplateDTO))
            {
                await _planTemplateDetailsGenerator.Generate(planTemplateDTO);
            }

            return Ok($"details/{planTemplateDTO.Name}-{planTemplateDTO.ParentPlanId.ToString()}.html");
        }

        [HttpPost]
        [DockyardAuthorize(Roles = Roles.Admin)]
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

            int found_templates = 0;
            int missed_templates = 0;

            foreach (var searchItemDto in searchResult.PlanTemplates)
            {
                var planTemplateDto = await _planTemplate.GetPlanTemplateDTO(fr8AccountId, searchItemDto.ParentPlanId);
                if (planTemplateDto == null)
                {
                    // if plan doesn't exist in MT let's remove it from index
                    await _searchProvider.Remove(searchItemDto.ParentPlanId);
                    missed_templates++;
                    continue;
                }
                found_templates++;
                var planTemplateCm = await _planTemplate.CreateOrUpdate(fr8AccountId, planTemplateDto);

                //if ownerId will be the last admin id who pushed the button. it therefore possible bugs 
                planTemplateCm.OwnerName = searchItemDto.Owner;
                
                await _searchProvider.CreateOrUpdate(planTemplateCm);
                await _planTemplateDetailsGenerator.Generate(planTemplateDto);
                await _webservicesPageGenerator.Generate(planTemplateCm, fr8AccountId);
            }
            watch.Stop();
            var elapsed = watch.Elapsed;
            var message = $"Page generator: templates found: {found_templates}, templates missed: {missed_templates}";
            var message2 = $"Page Generator elapsed time: {elapsed.Minutes} minutes, {elapsed.Seconds} seconds";
            Logger.Warn(message);
            Logger.Warn(message2);

            return Ok(message + "\n\r" + message2);
        }
    }
}