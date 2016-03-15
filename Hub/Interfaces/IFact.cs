using Data.Entities;
using Data.Interfaces;
using System.Collections.Generic;

namespace Hub.Interfaces
{
    public interface IFact
    {
        IList<FactDO> GetByObjectId(IUnitOfWork unitOfWork, string id);
    }
}
