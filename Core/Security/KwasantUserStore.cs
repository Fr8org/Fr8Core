using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;

namespace Core.Security
{
    class KwasantUserStore : IUserStore<DockyardAccountDO>, 
        IUserSecurityStampStore<DockyardAccountDO>, 
        IUserEmailStore<DockyardAccountDO>,
        IUserPasswordStore<DockyardAccountDO>
    {
        private readonly IUnitOfWork _uow;

        public KwasantUserStore(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Dispose()
        {
        }

        public async Task CreateAsync(DockyardAccountDO dockyardAccount)
        {
            _uow.UserRepository.Add(dockyardAccount);
        }

        public async Task UpdateAsync(DockyardAccountDO dockyardAccount)
        {
            
        }

        public async Task DeleteAsync(DockyardAccountDO dockyardAccount)
        {
            _uow.UserRepository.Remove(dockyardAccount);
        }

        public async Task<DockyardAccountDO> FindByIdAsync(string userId)
        {
            return _uow.UserRepository.GetByKey(userId);
        }

        public async Task<DockyardAccountDO> FindByNameAsync(string userName)
        {
            return _uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == userName);
        }

        public async Task SetSecurityStampAsync(DockyardAccountDO dockyardAccount, string stamp)
        {
            dockyardAccount.SecurityStamp = stamp;
        }

        public async Task<string> GetSecurityStampAsync(DockyardAccountDO dockyardAccount)
        {
            return dockyardAccount.SecurityStamp;
        }

        public async Task SetEmailAsync(DockyardAccountDO dockyardAccount, string email)
        {
            dockyardAccount.Email = email;
            dockyardAccount.EmailAddress.Address = email;
        }

        public async Task<string> GetEmailAsync(DockyardAccountDO dockyardAccount)
        {
            return dockyardAccount.EmailAddress.Address;
        }

        public async Task<bool> GetEmailConfirmedAsync(DockyardAccountDO dockyardAccount)
        {
            return dockyardAccount.EmailConfirmed;
        }

        public async Task SetEmailConfirmedAsync(DockyardAccountDO dockyardAccount, bool confirmed)
        {
            dockyardAccount.EmailConfirmed = confirmed;
        }

        public async Task<DockyardAccountDO> FindByEmailAsync(string email)
        {
            return _uow.UserRepository.FindOne(u => u.EmailAddress.Address == email);
        }

        #region Implementation of IUserPasswordStore<DockyardAccountDO,in string>

        public async Task SetPasswordHashAsync(DockyardAccountDO dockyardAccount, string passwordHash)
        {
            dockyardAccount.PasswordHash = passwordHash;
        }

        public async Task<string> GetPasswordHashAsync(DockyardAccountDO dockyardAccount)
        {
            return dockyardAccount.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(DockyardAccountDO dockyardAccount)
        {
            return !string.IsNullOrEmpty(dockyardAccount.PasswordHash);
        }

        #endregion
    }
}
