using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistryController : ApiController
    {

        [HttpGet]
        public IHttpActionResult Get(string userAccountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(uow, userAccountId);
                var list = manifestDescriptions.Select(m => new { m.Id, m.Name, m.Version, m.SampleJSON, m.Description, m.RegisteredBy }).ToList();

                return Ok(list);
            }
        }

        [HttpPost]
        public IHttpActionResult Post(ManifestDescriptionDTO description)
        {
            ManifestDescriptionCM manifestDescription = Mapper.Map<ManifestDescriptionCM>(description);
            manifestDescription.Id = NextId(description.UserAccountId);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.MultiTenantObjectRepository.Add(uow, manifestDescription, description.UserAccountId);

                uow.SaveChanges();
            }

            var model = Mapper.Map<ManifestDescriptionDTO>(manifestDescription);

            return Ok(model);
        }

        [HttpGet]
        [ActionName("checkVersionAndName")]
        public IHttpActionResult CheckVersionAndName(string versionAndName, string userAccountId)
        {
            string version = versionAndName.Split(':')[0];
            string name = versionAndName.Split(':')[1];

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var manifestDescription = uow.MultiTenantObjectRepository.Get<ManifestDescriptionCM>
                                                (uow, userAccountId, a => a.Version == version && a.Name == name);

                return Ok(manifestDescription == null);
            }
        }

        private string NextId(string userAccountId)
        {
            int result = 0;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(uow, userAccountId);
                if (!manifestDescriptions.Any())
                {
                    return result.ToString();
                }
                result = manifestDescriptions.Max(obj => int.Parse(obj.Id)) + 1;                
            }

            return result.ToString();
        }
    }
}
