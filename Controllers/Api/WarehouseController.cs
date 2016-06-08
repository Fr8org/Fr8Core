using System.Web.Http;
using StructureMap;
using Microsoft.AspNet.Identity;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
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
    }
}