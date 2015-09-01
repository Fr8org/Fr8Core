using System.Data.Entity;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Core.Security
{
    public class DockyardIdentityManager : UserManager<DockyardAccountDO>
    {
        public DockyardIdentityManager(IUnitOfWork unitOfWork)
            : this(new DockyardUserStore(unitOfWork))
        {
        }

        public DockyardIdentityManager(IUserStore<DockyardAccountDO> store)
            : base(store)
        {
            UserTokenProvider = new EmailTokenProvider<DockyardAccountDO>();
        }
    }
}
