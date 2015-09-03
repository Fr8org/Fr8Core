using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class UserEmails:IUserEmails
    {
        public IEnumerable<EmailAddressDO> GetUserEmails()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.UserEmailsRepository.GetAll();
            }
        }
       
    }
    public interface IUserEmails
    {
        IEnumerable<EmailAddressDO> GetUserEmails();
    }
}
