using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Interfaces;

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
            UserTokenProvider = new EmailTokenProvider<Fr8AccountDO>();
        }
    }
}
