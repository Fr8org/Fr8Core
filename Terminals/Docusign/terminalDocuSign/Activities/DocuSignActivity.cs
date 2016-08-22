using System;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public abstract class DocuSignActivity<TActivityUi> : TerminalActivity<TActivityUi> where TActivityUi : StandardConfigurationControlsCM
    {
        protected readonly IDocuSignManager DocuSignManager;
      
        protected DocuSignActivity(ICrateManager crateManager, IDocuSignManager docuSignManager)
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