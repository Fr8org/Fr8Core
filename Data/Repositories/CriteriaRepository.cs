using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    /// <summary>
    /// Repository to work with CriteriaDO entities.
    /// </summary>
    public class CriteriaRepository : GenericRepository<CriteriaDO>, ICriteriaRepository
    {
        public CriteriaRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    /// <summary>
    /// Repository interface to work with CriteriaDO entities.
    /// </summary>
    public interface ICriteriaRepository : IGenericRepository<CriteriaDO>
    {
    }
}
