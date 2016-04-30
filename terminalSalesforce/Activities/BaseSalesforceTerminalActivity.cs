using System;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using terminalSalesforce.Services;
using TerminalBase.BaseClasses;

namespace terminalSalesforce.Actions
{
    public abstract class BaseSalesforceTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseSalesforceTerminalActivity() : base(true)
        {
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return SalesforceAuthHelper.IsTokenInvalidation(ex);
        }
    }
}