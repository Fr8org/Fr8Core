using System;
using System.Configuration;
using AutoMapper;
using Data.Interfaces;
using StructureMap;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;
using System.Web.Http.Description;
using System.Collections.Generic;
using System.Net;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistryController : ApiController
    {
        private readonly IManifestRegistryMonitor _manifestRegistryMonitor;
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IFr8Account _fr8Account;

        public ManifestRegistryController(IManifestRegistryMonitor manifestRegistryMonitor, IFr8Account fr8Account, IRestfulServiceClient restfulServiceClient)
        {
            if (manifestRegistryMonitor == null)
            {
                throw new ArgumentNullException(nameof(manifestRegistryMonitor));
            }
            _fr8Account = fr8Account;
            _manifestRegistryMonitor = manifestRegistryMonitor;
            _restfulServiceClient = restfulServiceClient;
        }

        /// <summary>
        /// Retrieves the collection of manifests registered in the hub
        /// </summary>
        /// <response code="200">Collection of registered manifests</response>
        [HttpGet]
        [ResponseType(typeof(List<ManifestDescriptionDTO>))]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var systemUserAccount = _fr8Account.GetSystemUser();
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccount.UserName);
                var list = manifestDescriptions.Select(m => new ManifestDescriptionDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Version = m.Version,
                    SampleJSON = m.SampleJSON,
                    Description = m.Description,
                    RegisteredBy = m.RegisteredBy
                }).ToList();

                return Ok(list);
            }
        }

        [HttpGet]
        [ActionName("get_manifest_page_url")]
        public async Task<IHttpActionResult> GetManifestPageUrl(string manifestName)
        {
            return Ok(await _restfulServiceClient.PostAsync<string, string>(
                new Uri($"{CloudConfigurationManager.GetSetting("PlanDirectoryUrl")}/api/v1/page_generation/generate_manifest_page"),
                manifestName));
        }

        /// <summary>
        /// Retrieves URL of Google Form that is used to submit new manifests
        /// </summary>
        /// <response code="200">URL of Google Form</response>
        [HttpGet]
        [ResponseType(typeof(string))]
        public IHttpActionResult Submit()
        {
            return Ok(ConfigurationManager.AppSettings["ManifestSubmissionFormUrl"]);
        }
        /// <summary>
        /// Submits new manifest to manifest registry
        /// </summary>
        /// <remarks>
        /// if ManifestRegistryParams.version is empty gets the manifest description with given name and greatest version. Otherwise checks if there is any manifest description with given name and given version</remarks>
        /// <param name="description">Description of new manifest</param>
        /// <response code="200">Description of the submitted manifest</response>
        [HttpPost]
        [ResponseType(typeof(ManifestDescriptionDTO))]
        [DockyardAuthorize(Roles = Roles.Admin)]
        public IHttpActionResult Post(ManifestDescriptionDTO description)
        {
            ManifestDescriptionCM manifestDescription = Mapper.Map<ManifestDescriptionCM>(description);
            manifestDescription.Id = NextId();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var systemUserAccount = _fr8Account.GetSystemUser();
                uow.MultiTenantObjectRepository.Add(manifestDescription, systemUserAccount.UserName);

                uow.SaveChanges();
            }

            var model = Mapper.Map<ManifestDescriptionDTO>(manifestDescription);

            return Ok(model);
        }
        /// <summary>
        /// Starts internal plan that monitors new manifest submissions and creates a JIRA for them
        /// </summary>
        [HttpPost]
        [DockyardAuthorize(Roles = Roles.Admin)]
        [SwaggerResponse(HttpStatusCode.OK, "Manifest monitoring plan was successfully launched")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> RunMonitoring()
        {
            //We don't need to wait for the result as the purpose of this method is just to initiate a start (as a double-check measure)
            await _manifestRegistryMonitor.StartMonitoringManifestRegistrySubmissions();
            return Ok();
        }

        private string NextId()
        {
            int result = 1;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var systemUserAccount = _fr8Account.GetSystemUser();
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccount.UserName);
                if (!manifestDescriptions.Any())
                {
                    return result.ToString();
                }
                result = manifestDescriptions.Select(x => int.Parse(x.Id)).OrderByDescending(x => x).First() + 1;                
            }
            return result.ToString();
        }        
    }
}
