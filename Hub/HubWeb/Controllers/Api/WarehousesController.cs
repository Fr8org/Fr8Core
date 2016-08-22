using System.Net;
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
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class WarehousesController : ApiController
    {
        /// <summary>
        /// Retrieves objects from Fr8 warehouse based on filter specified
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="query">Query filter</param>
        /// <response code="200">Collection of queried objects</response>
        /// <response code="403">Unauthorized request</response>
        [Fr8TerminalAuthentication(true)]
        [HttpPost]
        [ResponseType(typeof(object[]))]
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
        /// <summary>
        /// Deletes objects from Fr8 warehouse based on filter specified
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="query">Query filter</param>
        [Fr8TerminalAuthentication(true)]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Objects were succesfully deleted")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult Delete(QueryDTO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var mtTypeRef = uow.MultiTenantObjectRepository.FindTypeReference(query.Name);
                var queryBuilder = MTSearchHelper.CreateQueryProvider(mtTypeRef.ClrType);
                queryBuilder.Delete(
                    uow,
                    User.Identity.GetUserId(),
                    query.Criteria
                    );
                uow.SaveChanges();
                return Ok();
            }
        }
        /// <summary>
        /// Saves specified crates into Fr8 warehouse
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="crateStorageDto">Crates to store in Fr8 warehouse</param>
        [Fr8TerminalAuthentication(true)]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Objects were succesfully saved")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult Post(CrateStorageDTO crateStorageDto)
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