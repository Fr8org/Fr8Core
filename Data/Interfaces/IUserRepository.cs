using System;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUserRepository : IGenericRepository<DockyardAccountDO>
    {
        DockyardAccountDO UpdateUserCredentials(String emailAddress, String userName = null, String password = null);
        DockyardAccountDO UpdateUserCredentials(EmailAddressDO emailAddressDO, String userName = null, String password = null);
        DockyardAccountDO UpdateUserCredentials(DockyardAccountDO dockyardAccountDO, String userName = null, String password = null);
        DockyardAccountDO GetOrCreateUser(String emailAddress);
        DockyardAccountDO GetOrCreateUser(EmailAddressDO emailAddressDO);
    }
}