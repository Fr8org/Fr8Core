using AutoMapper;
using Data.Interfaces;
using StructureMap;
using System.Linq;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Utilities;

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistryController : ApiController
    {
        private static string systemUserAccountId = ObjectFactory.GetInstance<IConfigRepository>().Get("SystemUserEmail");
        
        [HttpGet]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccountId);
                var list = manifestDescriptions.Select(m => new { m.Id, m.Name, m.Version, m.SampleJSON, m.Description, m.RegisteredBy }).ToList();

                return Ok(list);
            }
        }

        [HttpPost]
        public IHttpActionResult Post(ManifestDescriptionDTO description)
        {
            ManifestDescriptionCM manifestDescription = Mapper.Map<ManifestDescriptionCM>(description);
            manifestDescription.Id = NextId();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.MultiTenantObjectRepository.Add(manifestDescription, systemUserAccountId);

                uow.SaveChanges();
            }

            var model = Mapper.Map<ManifestDescriptionDTO>(manifestDescription);

            return Ok(model);
        }

        [HttpGet]
        [ActionName("checkVersionAndName")]
        public IHttpActionResult CheckVersionAndName(string version, string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccountId);
                var isInDB = manifestDescriptions.Any(md => md.Name == name && md.Version == version);
                BoolValue result = new BoolValue  { Value = !isInDB };

                return Ok(result);
            }
        }

        [HttpGet]
        [ActionName("getDescriptionWithMaxVersion")]
        public IHttpActionResult GetDescriptionWithMaxVersion(string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccountId);
                var descriptions = manifestDescriptions.Where(md => md.Name == name).ToArray();

                var result = descriptions.First();
                string maxVersion = result.Version;

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
        }

        private string NextId()
        {
            int result = 1;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccountId);
                if (!manifestDescriptions.Any())
                {
                    return result.ToString();
                }
                result = int.Parse(manifestDescriptions.OrderByDescending(d => d.Id).First().Id) + 1;                
            }

            return result.ToString();
        }

        class BoolValue
        {
            public bool Value { get; set; }
        }
    }
}
