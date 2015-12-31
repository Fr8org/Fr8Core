using System.Collections.Generic;
using Data.Crates;

namespace Data.Interfaces.Manifests
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
