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

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistriesController : ApiController
    {
        private readonly IManifestRegistryMonitor _manifestRegistryMonitor;

        private readonly string _systemUserAccountId;

        //TODO: uncomment this constructor once constructor injection is enabled for HubWeb controllers
        //public ManifestRegistriesController(IManifestRegistryMonitor manifestRegistryMonitor, IConfigRepository configRepository)
        //{
        //    if (manifestRegistryMonitor == null)
        //    {
        //        throw new ArgumentNullException(nameof(manifestRegistryMonitor));
        //    }
        //    if (configRepository == null)
        //    {
        //        throw new ArgumentNullException(nameof(configRepository));
        //    }
        //    _systemUserAccountId = configRepository.Get("SystemUserEmail");
        //    _manifestRegistryMonitor = manifestRegistryMonitor;
        //}

        //TODO: remove this construcotr once constructor injection is enabled for HubWeb controllers
        public ManifestRegistriesController()
        {
            _systemUserAccountId = ObjectFactory.GetInstance<IConfigRepository>().Get("SystemUserEmail");
            _manifestRegistryMonitor = ObjectFactory.GetInstance<IManifestRegistryMonitor>();
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(_systemUserAccountId);
                var list = manifestDescriptions.Select(m => new { m.Id, m.Name, m.Version, m.SampleJSON, m.Description, m.RegisteredBy }).ToList();

                return Ok(list);
            }
        }

        [HttpGet]
        public IHttpActionResult Submit()
        {
            return Ok(ConfigurationManager.AppSettings["ManifestSubmissionFormUrl"]);
        }


        [HttpPost]
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

        [HttpPost]
        [DockyardAuthorize(Roles = Roles.Admin)]
        public async Task<IHttpActionResult> RunMonitoring()
        {
            //We don't need to wait for the result as the purpose of this method is just to initiate a start (as a double-check measure)
            await _manifestRegistryMonitor.StartMonitoringManifestRegistrySubmissions();
            return Ok();
        }


        [HttpPost]
        [ActionName("query")]
        public IHttpActionResult Query([FromBody]  ManifestRegistryParams data)
        {
            object result = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (data.version.IsNullOrEmpty())
                // getDescriptionWithLastVersion
                {
                    var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(_systemUserAccountId);
                    var descriptions = manifestDescriptions.Where(md => md.Name == data.name).ToArray();

                    result = descriptions.First();
                    string maxVersion = ((ManifestDescriptionCM)result).Version;

                    foreach (var description in descriptions)
                    {
                        if (description.Version.CompareTo(maxVersion) > 0)
                        {
                            result = description;
                            maxVersion = description.Version;
                        }
                    }

                    return Ok(result);
                }

                else
                // checkVersionAndName
                {
                    var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(_systemUserAccountId);
                    var isInDB = manifestDescriptions.Any(md => md.Name == data.name && md.Version == data.version);
                    result = new { Value = !isInDB };

                    return Ok(result);
                }
                
            }
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
                result = int.Parse(manifestDescriptions.OrderByDescending(d => d.Id).First().Id) + 1;                
            }

            return result.ToString();
        }        
    }
}
