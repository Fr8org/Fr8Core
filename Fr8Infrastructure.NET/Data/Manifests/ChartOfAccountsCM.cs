using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class ChartOfAccountsCM : Manifest
    {
        public List<AccountDTO> Accounts { get; set; } 
        public ChartOfAccountsCM() : base(MT.ChartOfAccounts)
        {
        }
    }
    /// <summary>
    /// This class is currently used for Accounting
    /// </summary>
    public class AccountDTO
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}
