using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IAspNetUserClaimDO : IBaseDO
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
    }
}
