using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;

namespace TerminalBase.BaseClasses
{
    public class SimpleActionBase : BaseTerminalAction
    {
        protected CrateStorage CurrentActionStorage { get; private set; }
        protected CrateStorage CurrentPayloadStorage { get; private set; }

        protected virtual void Run(AuthorizationTokenDO token)
        {
        }

        protected virtual void Initialize(AuthorizationTokenDO token)
        {
        }

        protected virtual void Configure(AuthorizationTokenDO token)
        {
        }
    }
    
    public class SimpleActionBase<TUi> : SimpleActionBase
        where TUi : StandardConfigurationControlsCM, new()
    {
        protected TUi Ui { get; set; }
    }
}
