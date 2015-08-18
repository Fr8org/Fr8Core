using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    /// <summary>
    /// Repository to work with PluginDO entities.
    /// </summary>
    public class PluginRepository : GenericRepository<PluginDO>, IPluginRepository
    {
        public PluginRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    /// <summary>
    /// Repository interface to work with PluginDO entities.
    /// </summary>
    public interface IPluginRepository : IGenericRepository<PluginDO>
    {
    }
}
