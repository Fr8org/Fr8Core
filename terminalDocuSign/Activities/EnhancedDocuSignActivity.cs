using System;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

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