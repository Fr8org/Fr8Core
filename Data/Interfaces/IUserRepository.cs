using System;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUserRepository : IGenericRepository<Fr8AccountDO>
    {
        Fr8AccountDO UpdateUserCredentials(String emailAddress, String userName = null, String password = null);
        Fr8AccountDO UpdateUserCredentials(EmailAddressDO emailAddressDO, String userName = null, String password = null);
        Fr8AccountDO UpdateUserCredentials(Fr8AccountDO dockyardAccountDO, String userName = null, String password = null);
        Fr8AccountDO GetOrCreateUser(String emailAddress);
        Fr8AccountDO GetOrCreateUser(EmailAddressDO emailAddressDO);
    }
}