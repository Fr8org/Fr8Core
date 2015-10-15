using System.Data.Entity;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Core.Security
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
            UserTokenProvider = new EmailTokenProvider<Fr8AccountDO>();
        }
    }
}
