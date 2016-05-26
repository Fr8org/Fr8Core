using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Microsoft.AspNet.Identity;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Hub.Infrastructure;
using Hub.Managers;
using Hub.Services;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class WarehouseController : ApiController
    {
        private ICrateManager _crateManager;

        public WarehouseController()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public async Task<IHttpActionResult> Post(string userId, List<CrateDTO> crates)
        {
            var result = new List<CrateDTO>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var crate in crates)
                {
                    DocuSignEnvelopeCM_v2 manifest = (DocuSignEnvelopeCM_v2)_crateManager.FromDto(crate).Get();
                    //TODO: allow specifying manifest type as string and make MT aware of primary keys
                    var recordOfEnvelopeEvents = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM_v2>(userId, a => a.EnvelopeId == manifest.EnvelopeId);
                    foreach (var stored_manifest in recordOfEnvelopeEvents)
                    {
                        result.Add(_crateManager.ToDto(Fr8Data.Crates.Crate.FromContent("RecordedEnvelope", stored_manifest, Fr8Data.States.AvailabilityType.RunTime)));
                    }
                }
            }
            return Ok(result);
        }

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