using Data.Interfaces;
using Data.Entities;
using Data.Validations;
using FluentValidation;

namespace Data.Repositories
{
    public class ContainerRepository : GenericRepository<ContainerDO>, IContainerRepository
    {
      
        internal ContainerRepository(IUnitOfWork uow)
            : base(uow)
        {
          
        }




    }
    public interface IContainerRepository : IGenericRepository<ContainerDO>
    {
        
    }



}
