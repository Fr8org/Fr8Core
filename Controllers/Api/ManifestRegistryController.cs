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
                
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(uow, systemUserAccountId);
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
                uow.MultiTenantObjectRepository.Add(uow, manifestDescription, systemUserAccountId);

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
                // TODO: change this to check version too, expression can be only unary on key property! 
                // TODO: Need to upgrade MultiTennantObjectRepository
                var manifestDescription = uow.MultiTenantObjectRepository.Get<ManifestDescriptionCM>
                                                (uow, systemUserAccountId, a => a.Name == name);
                
                return Ok(manifestDescription == null);
            }
        }

        private string NextId()
        {
            int result = 0;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(uow, systemUserAccountId);
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
