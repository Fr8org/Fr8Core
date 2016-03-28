using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class OrganizationRepository : GenericRepository<OrganizationDO>, IOrganizationRepository
    {
        public OrganizationRepository(IUnitOfWork uow)
            :base(uow)
        {
        }
    }

    public interface IOrganizationRepository : IGenericRepository<OrganizationDO>
    {
    }
}
