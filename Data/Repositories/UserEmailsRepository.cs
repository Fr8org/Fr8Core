using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class UserEmailsRepository : GenericRepository<EmailAddressDO>, IUserEmailsRepository
    {
        internal UserEmailsRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
        public IEnumerable<EmailAddressDO> getUserEmails()
        {
            return UnitOfWork.UserEmailsRepository.GetAll();
            
        }
    }

    public interface IUserEmailsRepository : IGenericRepository<EmailAddressDO>
    {
        IEnumerable<EmailAddressDO> getUserEmails();
    }
}
