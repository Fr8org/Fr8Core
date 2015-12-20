using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Interfaces;
using System;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace Hub.Security
{
    public class DockyardIdentityManager : UserManager<Fr8AccountDO>
    {
        public DockyardIdentityManager(IUnitOfWork unitOfWork)
            : this(new DockyardUserStore(unitOfWork))
        {
        }

        public DockyardIdentityManager(IUserStore<Fr8AccountDO> store)
            : base(store)
        {
            var dataProtectionProvider = new DpapiDataProtectionProvider("fr8");
            this.UserTokenProvider = new DataProtectorTokenProvider<Fr8AccountDO>(dataProtectionProvider.Create())
            {
                // Expiration time.
                TokenLifespan = TimeSpan.FromHours(24)
            };
        }
    }
}
