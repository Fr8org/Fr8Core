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
        // DPAPI by design across ALL Windows OS has been machine specific, because the Cryptography Key
        // it uses for generating cipher text is machine specific. 
        // This Cryptography Key is stored in the Windows Registry of that specific machine.
        // So changed the implementation: http://tech.trailmax.info/2014/06/asp-net-identity-and-cryptographicexception-when-running-your-site-on-microsoft-azure-web-sites/
        public static IDataProtectionProvider DataProtectionProvider { get; set; }

        public DockyardIdentityManager(IUnitOfWork unitOfWork)
            : this(new DockyardUserStore(unitOfWork))
        {
        }

        public DockyardIdentityManager(IUserStore<Fr8AccountDO> store)
            : base(store)
        {
            var dataProtectionProvider = DockyardIdentityManager.DataProtectionProvider;
            this.UserTokenProvider = new DataProtectorTokenProvider<Fr8AccountDO>(dataProtectionProvider.Create())
            {
                // Expiration time.
                TokenLifespan = TimeSpan.FromHours(24)
            };
        }
    }
}
