using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    class PageDefinitionRepository : GenericRepository<PageDefinitionDO>, IPageDefinitionRepository
    {
        public PageDefinitionRepository(IUnitOfWork uow) : base(uow) { }
    }

    public interface IPageDefinitionRepository : IGenericRepository<PageDefinitionDO> { }
}
