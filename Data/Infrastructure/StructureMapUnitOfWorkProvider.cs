using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure
{
    public class StructureMapUnitOfWorkProvider : IUnitOfWorkProvider
    {
        public IUnitOfWork GetNewUnitOfWork()
        {
            return ObjectFactory.GetInstance<IUnitOfWork>();
        }
    }
}
