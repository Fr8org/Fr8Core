using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
	public class WebServiceRepository : GenericRepository<WebServiceDO>, IWebServiceRepository
	{
		public WebServiceRepository(IUnitOfWork uow) : base(uow)
		{

		}
	}

	public interface IWebServiceRepository : IGenericRepository<WebServiceDO>
	{
		
	}
}