using System;
using Fr8Data.Manifests;
using StructureMap;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;

namespace terminalDocuSign.Activities
{
    public abstract class EnhancedDocuSignActivity<TActivityUi> : EnhancedTerminalActivity<TActivityUi> where TActivityUi : StandardConfigurationControlsCM
    {
        protected readonly IDocuSignManager DocuSignManager;

        protected readonly IDocuSignFolders DocuSignFolders;
        //TODO: remove this constructor after introducing constructor injection
        protected EnhancedDocuSignActivity() : this(ObjectFactory.GetInstance<IDocuSignManager>(), ObjectFactory.GetInstance<IDocuSignFolders>())
        {
        }

        protected EnhancedDocuSignActivity(IDocuSignManager docuSignManager, IDocuSignFolders docuSignFolders) : base(true)
        {
            if (docuSignManager == null)
            {
                throw new ArgumentNullException(nameof(docuSignManager));
            }
            if (docuSignFolders == null)
            {
                throw new ArgumentNullException(nameof(docuSignFolders));
            }
            DocuSignManager = docuSignManager;
            DocuSignFolders = docuSignFolders;
        } 

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN");
        }
    }
}