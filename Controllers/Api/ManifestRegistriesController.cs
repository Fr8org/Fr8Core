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
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistriesController : ApiController
    {
        private readonly IManifestRegistryMonitor _manifestRegistryMonitor;

        private readonly string _systemUserAccountId;

        public ManifestRegistriesController(IManifestRegistryMonitor manifestRegistryMonitor, IConfigRepository configRepository)
        {
            if (manifestRegistryMonitor == null)
            {
                throw new ArgumentNullException(nameof(manifestRegistryMonitor));
            }
            if (configRepository == null)
            {
                throw new ArgumentNullException(nameof(configRepository));
            }
            _systemUserAccountId = configRepository.Get("SystemUserEmail");
            _manifestRegistryMonitor = manifestRegistryMonitor;
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
                
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(_systemUserAccountId);
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
                uow.MultiTenantObjectRepository.Add(manifestDescription, _systemUserAccountId);

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
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(_systemUserAccountId);
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
