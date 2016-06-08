using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using StructureMap;
using Hub.Infrastructure;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Controllers.Api
{
    [RoutePrefix("plan_templates")]
    public class PlanTemplatesController : ApiController
    {
        private readonly IPlanTemplate _planTemplate;
        private readonly ISearchProvider _searchProvider;
        private readonly ITagGenerator _tagGenerator;

        public PlanTemplatesController()
        {
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
            _searchProvider = ObjectFactory.GetInstance<ISearchProvider>();
            _tagGenerator = ObjectFactory.GetInstance<ITagGenerator>();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public Task<IHttpActionResult> Post(PublishPlanTemplateDTO dto)
        {
            return ExceptionWrapper(async () =>
            {
                var fr8AccountId = User.Identity.GetUserId();

                var planTemplateCM = await _planTemplate.CreateOrUpdate(fr8AccountId, dto);

                var tags = await _tagGenerator.GetTags(planTemplateCM, fr8AccountId);

                await _searchProvider.CreateOrUpdate(planTemplateCM);

                //TODO: update page definitions

                return Ok();
            });
        }

        [HttpDelete]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public Task<IHttpActionResult> Delete(Guid id)
        {
            return ExceptionWrapper(async () =>
            {
                var fr8AccountId = User.Identity.GetUserId();
                var planTemplateCM = await _planTemplate.Get(fr8AccountId, id);

                if (planTemplateCM != null)
                {
                    await _planTemplate.Remove(fr8AccountId, id);
                    await _searchProvider.Remove(id);
                }

                return Ok();
            });
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateDTO = await _planTemplate.Get(fr8AccountId, id);

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
        public Task<IHttpActionResult> CreatePlan(Guid id)
        {
            return ExceptionWrapper(async () =>
            {
                var fr8AccountId = User.Identity.GetUserId();

                var hmacService = ObjectFactory.GetInstance<IHMACService>();
                var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

                var planTemplateDTO = await _planTemplate.Get(fr8AccountId, id);

                var uri = new Uri(CloudConfigurationManager.GetSetting("HubUrl") + "/api/v1/plans/load");
                var headers = await hmacService.GenerateHMACHeader(
                    uri,
                    "PlanDirectory",
                    CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
                    User.Identity.GetUserId(),
                    planTemplateDTO.PlanContents
                );

                var plan = await client.PostAsync<JToken, PlanEmptyDTO>(
                    uri, planTemplateDTO.PlanContents, headers: headers);

                return Ok(new { RedirectUrl = CloudConfigurationManager.GetSetting("HubUrl") + "/dashboard/plans/" + plan.Id.ToString() + "/builder?viewMode=plan" });
            });
        }

        // Added for PD <-> Hub debugging purposes only, to be removed in future.
        private Task<IHttpActionResult> ExceptionWrapper(Func<Task<IHttpActionResult>> handler)
        {
            try
            {
                return handler();
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();

                while (ex != null)
                {
                    sb.AppendLine(ex.Message);
                    sb.AppendLine(ex.StackTrace);

                    ex = ex.InnerException;
                }

                return Task.FromResult<IHttpActionResult>(Ok(new { exception = sb.ToString() }));
            }
        }
    }
}