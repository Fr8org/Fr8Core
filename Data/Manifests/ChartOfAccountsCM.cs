using System.Collections.Generic;
using Fr8Data.Crates;

namespace Fr8Data.Manifests
{
    public class ChartOfAccountsCM : Manifest
    {
        public List<AccountDTO> Accounts { get; set; } 
        public ChartOfAccountsCM() : base(Constants.MT.ChartOfAccounts)
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
