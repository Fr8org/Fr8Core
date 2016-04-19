using System;
using Google.GData.Client;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;
using terminalGoogle.Services;

namespace terminalGoogle.Actions
{
    public abstract class BaseGoogleTerminalActivity<T> : EnhancedTerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected BaseGoogleTerminalActivity() : base(true)
        {
        }

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return GoogleAuthHelper.IsTokenInvalidation(ex);
        }
    }
}