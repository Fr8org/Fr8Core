using System;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public abstract class EnhancedDocuSignActivity<TActivityUi> : EnhancedTerminalActivity<TActivityUi> where TActivityUi : StandardConfigurationControlsCM
    {
        protected readonly IDocuSignManager DocuSignManager;
      
        protected EnhancedDocuSignActivity(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager)
        {
            if (docuSignManager == null)
            {
                throw new ArgumentNullException(nameof(docuSignManager));
            }

            DocuSignManager = docuSignManager;
        } 

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN");
        }
    }
}