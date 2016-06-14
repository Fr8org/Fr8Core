using System.Web.Http;
using StructureMap;
using Microsoft.AspNet.Identity;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Infrastructure;
using Hub.Services;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class WarehouseController : ApiController
    {
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public IHttpActionResult Query(QueryDTO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mtTypeRef = uow.MultiTenantObjectRepository.FindTypeReference(query.Name);
                var queryBuilder = MTSearchHelper.CreateQueryProvider(mtTypeRef.ClrType);
                var foundObjects = queryBuilder.Query(
                    uow,
                    User.Identity.GetUserId(),
                    query.Criteria
                )
                .ToArray();

                return Ok(foundObjects);
            }
        }
        
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public IHttpActionResult Add(CrateStorageDTO crateStorageDto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var crateStorage = CrateStorageSerializer.Default.ConvertFromDto(crateStorageDto);
                var userId = User.Identity.GetUserId();

                foreach (var crate in crateStorage)
                {
                    if (crate.IsKnownManifest)
                    {
                        uow.MultiTenantObjectRepository.AddOrUpdate(userId, (Manifest) crate.Get());
                    }
                }

                uow.SaveChanges();
            }

            return Ok();
        }
    }
}