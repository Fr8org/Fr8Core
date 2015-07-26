using System.Data.Entity;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Core.Security
{
    public class KwasantUserManager : UserManager<DockyardAccountDO>
    {
        public KwasantUserManager(IUnitOfWork unitOfWork)
            : this(new KwasantUserStore(unitOfWork))
        {
        }

        public KwasantUserManager(IUserStore<DockyardAccountDO> store)
            : base(store)
        {
            UserTokenProvider = new EmailTokenProvider<DockyardAccountDO>();
        }
    }
}
