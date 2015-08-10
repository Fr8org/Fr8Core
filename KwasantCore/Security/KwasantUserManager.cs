using System.Data.Entity;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace KwasantCore.Security
{
    public class KwasantUserManager : UserManager<UserDO>
    {
        public KwasantUserManager(IUnitOfWork unitOfWork)
            : this(new KwasantUserStore(unitOfWork))
        {
        }

        public KwasantUserManager(IUserStore<UserDO> store)
            : base(store)
        {
            UserTokenProvider = new EmailTokenProvider<UserDO>();
        }
    }
}
