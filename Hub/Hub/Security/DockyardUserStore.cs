using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Security
{
    class DockyardUserStore : IUserStore<Fr8AccountDO>,
        IUserSecurityStampStore<Fr8AccountDO>,
        IUserEmailStore<Fr8AccountDO>,
        IUserPasswordStore<Fr8AccountDO>,
        IUserClaimStore<Fr8AccountDO>
    {
        private readonly IUnitOfWork _uow;

        public DockyardUserStore(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Dispose()
        {
        }

        public Task CreateAsync(Fr8AccountDO dockyardAccount)
        {
            _uow.UserRepository.Add(dockyardAccount);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(0);
        }

        public Task DeleteAsync(Fr8AccountDO dockyardAccount)
        {
            _uow.UserRepository.Remove(dockyardAccount);
            return Task.FromResult(0);
        }

        public Task<Fr8AccountDO> FindByIdAsync(string userId)
        {
            return Task.FromResult(_uow.UserRepository.GetByKey(userId));
        }

        public Task<Fr8AccountDO> FindByNameAsync(string userName)
        {
            return Task.FromResult(_uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == userName));
        }

        public Task SetSecurityStampAsync(Fr8AccountDO dockyardAccount, string stamp)
        {
            dockyardAccount.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.SecurityStamp);
        }

        public Task SetEmailAsync(Fr8AccountDO dockyardAccount, string email)
        {
            dockyardAccount.Email = email;
            dockyardAccount.EmailAddress.Address = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.EmailAddress.Address);
        }

        public Task<bool> GetEmailConfirmedAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(Fr8AccountDO dockyardAccount, bool confirmed)
        {
            dockyardAccount.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<Fr8AccountDO> FindByEmailAsync(string email)
        {
            return Task.FromResult(_uow.UserRepository.FindOne(u => u.EmailAddress.Address == email));
        }

        #region Implementation of IUserPasswordStore<DockyardAccountDO,in string>

        public Task SetPasswordHashAsync(Fr8AccountDO dockyardAccount, string passwordHash)
        {
            dockyardAccount.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(dockyardAccount.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(Fr8AccountDO dockyardAccount)
        {
            return Task.FromResult(!string.IsNullOrEmpty(dockyardAccount.PasswordHash));
        }

        #endregion

        #region Implementations of IUserClaimStore<Fr8AccountDO> 

        public Task<IList<Claim>> GetClaimsAsync(Fr8AccountDO user)
        {
            var claims = _uow.AspNetUserClaimsRepository.GetQuery().Where(x => x.UserId == user.Id).ToList();
            return Task.FromResult((IList<Claim>)claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList());
        }

        public Task AddClaimAsync(Fr8AccountDO user, Claim claim)
        {
            _uow.AspNetUserClaimsRepository.Add(new AspNetUserClaimDO()
            {
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(Fr8AccountDO user, Claim claim)
        {
            var userClaim =_uow.AspNetUserClaimsRepository.GetQuery().FirstOrDefault(x => x.UserId == user.Id && x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            if (userClaim != null)
            {
                _uow.AspNetUserClaimsRepository.Remove(userClaim);
            }
            return Task.FromResult(0);
        }

        #endregion
    }
}
