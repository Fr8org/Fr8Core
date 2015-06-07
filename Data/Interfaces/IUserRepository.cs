using System;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUserRepository : IGenericRepository<UserDO>
    {
        UserDO UpdateUserCredentials(String emailAddress, String userName = null, String password = null);
        UserDO UpdateUserCredentials(EmailAddressDO emailAddressDO, String userName = null, String password = null);
        UserDO UpdateUserCredentials(UserDO userDO, String userName = null, String password = null);
        UserDO GetOrCreateUser(String emailAddress);
        UserDO GetOrCreateUser(EmailAddressDO emailAddressDO);
    }
}