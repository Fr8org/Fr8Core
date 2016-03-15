using Data.Entities;
using Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Hub.Services
{
    public class Fact : Hub.Interfaces.IFact
    {
        public IList<FactDO> GetByObjectId(IUnitOfWork unitOfWork, string id)
        {
            var query = unitOfWork.FactRepository.GetQuery();
            return query.Where(fact => fact.ObjectId == id).ToList();
        }
    }
}
