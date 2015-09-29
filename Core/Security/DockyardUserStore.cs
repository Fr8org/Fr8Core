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
    class DockyardUserStore : IUserStore<DockyardAccountDO>, 
        IUserSecurityStampStore<DockyardAccountDO>, 
        IUserEmailStore<DockyardAccountDO>,
        IUserPasswordStore<DockyardAccountDO>
    {
        private readonly IUnitOfWork _uow;

        public DockyardUserStore(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Dispose()
        {
        }

        public Task CreateAsync(DockyardAccountDO dockyardAccount)
        {
            _uow.UserRepository.Add(dockyardAccount);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(0);
        }

        public Task DeleteAsync(DockyardAccountDO dockyardAccount)
        {
            _uow.UserRepository.Remove(dockyardAccount);
            return Task.FromResult(0);
        }

        public Task<DockyardAccountDO> FindByIdAsync(string userId)
        {
            return Task.FromResult(_uow.UserRepository.GetByKey(userId));
        }

        public Task<DockyardAccountDO> FindByNameAsync(string userName)
        {
            return Task.FromResult(_uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == userName));
        }

        public Task SetSecurityStampAsync(DockyardAccountDO dockyardAccount, string stamp)
        {
            dockyardAccount.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.SecurityStamp);
        }

        public Task SetEmailAsync(DockyardAccountDO dockyardAccount, string email)
        {
            dockyardAccount.Email = email;
            dockyardAccount.EmailAddress.Address = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.EmailAddress.Address);
        }

        public Task<bool> GetEmailConfirmedAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(DockyardAccountDO dockyardAccount, bool confirmed)
        {
            dockyardAccount.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<DockyardAccountDO> FindByEmailAsync(string email)
        {
            return Task.FromResult(_uow.UserRepository.FindOne(u => u.EmailAddress.Address == email));
        }

        #region Implementation of IUserPasswordStore<DockyardAccountDO,in string>

        public Task SetPasswordHashAsync(DockyardAccountDO dockyardAccount, string passwordHash)
        {
            dockyardAccount.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(DockyardAccountDO dockyardAccount)
        {
            return Task.FromResult(!string.IsNullOrEmpty(dockyardAccount.PasswordHash));
        }

        #endregion
    }
}
