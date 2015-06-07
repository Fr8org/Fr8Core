using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;

namespace KwasantCore.Security
{
    class KwasantUserStore : IUserStore<UserDO>, 
        IUserSecurityStampStore<UserDO>, 
        IUserEmailStore<UserDO>,
        IUserPasswordStore<UserDO>
    {
        private readonly IUnitOfWork _uow;

        public KwasantUserStore(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Dispose()
        {
        }

        public async Task CreateAsync(UserDO user)
        {
            _uow.UserRepository.Add(user);
        }

        public async Task UpdateAsync(UserDO user)
        {
            
        }

        public async Task DeleteAsync(UserDO user)
        {
            _uow.UserRepository.Remove(user);
        }

        public async Task<UserDO> FindByIdAsync(string userId)
        {
            return _uow.UserRepository.GetByKey(userId);
        }

        public async Task<UserDO> FindByNameAsync(string userName)
        {
            return _uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == userName);
        }

        public async Task SetSecurityStampAsync(UserDO user, string stamp)
        {
            user.SecurityStamp = stamp;
        }

        public async Task<string> GetSecurityStampAsync(UserDO user)
        {
            return user.SecurityStamp;
        }

        public async Task SetEmailAsync(UserDO user, string email)
        {
            user.Email = email;
            user.EmailAddress.Address = email;
        }

        public async Task<string> GetEmailAsync(UserDO user)
        {
            return user.EmailAddress.Address;
        }

        public async Task<bool> GetEmailConfirmedAsync(UserDO user)
        {
            return user.EmailConfirmed;
        }

        public async Task SetEmailConfirmedAsync(UserDO user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
        }

        public async Task<UserDO> FindByEmailAsync(string email)
        {
            return _uow.UserRepository.FindOne(u => u.EmailAddress.Address == email);
        }

        #region Implementation of IUserPasswordStore<UserDO,in string>

        public async Task SetPasswordHashAsync(UserDO user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
        }

        public async Task<string> GetPasswordHashAsync(UserDO user)
        {
            return user.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(UserDO user)
        {
            return !string.IsNullOrEmpty(user.PasswordHash);
        }

        #endregion
    }
}
