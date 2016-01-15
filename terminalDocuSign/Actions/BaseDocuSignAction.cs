using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class BaseDocuSignAction : BaseTerminalAction
    {
        public virtual async System.Threading.Tasks.Task<Data.Entities.ActionDO> Activate(Data.Entities.ActionDO curActionDO, Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(null);

            return await Task.FromResult<ActionDO>(curActionDO);
        }
    }
}