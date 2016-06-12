using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System.Linq;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Infrastructure;
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

        /// <summary>
        /// Returns List of facts about ObjectId
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Fr8ApiAuthorize]
        [HttpPost]
        [ActionName("query")]
        // /facts/query 
        public IHttpActionResult ProcessQuery(FactDO query)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //TODO: verify objectId owned by current user? i.e. requested container 

                var facts = _fact.GetByObjectId(uow, query.ObjectId);
                return Ok(facts.Select(Mapper.Map<FactDTO>));
            };
        }
    }
}
