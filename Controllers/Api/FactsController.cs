using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Infrastructure;
using StructureMap;
using InternalInterface = Hub.Interfaces;

namespace HubWeb.Controllers.Api
{
    public class FactsController : ApiController
    {
        private readonly InternalInterface.IFact _fact;

        public FactsController()
        {
            _fact = ObjectFactory.GetInstance<InternalInterface.IFact>();
        }

        [Fr8ApiAuthorize]
        [HttpPost]
        [ActionName("query")]
        // /facts/query 
        public IHttpActionResult ProcessQuery(FactDO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var facts = _fact.GetByObjectId(uow, query.ObjectId);
                return Ok(facts.Select(Mapper.Map<HistoryItemDTO>));
            };
        }
    }
}
