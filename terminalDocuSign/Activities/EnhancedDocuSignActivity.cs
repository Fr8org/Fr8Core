using System;
using Fr8Data.Manifests;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;

namespace terminalDocuSign.Activities
{
    public abstract class EnhancedDocuSignActivity<TActivityUi> : EnhancedTerminalActivity<TActivityUi> where TActivityUi : StandardConfigurationControlsCM
    {
        protected readonly IDocuSignManager DocuSignManager;
        //TODO: remove this constructor after introducing constructor injection
        protected EnhancedDocuSignActivity() : this(ObjectFactory.GetInstance<IDocuSignManager>())
        {
        }

        protected EnhancedDocuSignActivity(IDocuSignManager docuSignManager) : base(true)
        {
            if (docuSignManager == null)
            {
                throw new ArgumentNullException(nameof(docuSignManager));
            }
            DocuSignManager = docuSignManager;
        } 

        protected override bool IsTokenInvalidation(Exception ex)
        {
            return ex.Message.Contains("AUTHORIZATION_INVALID_TOKEN");
        }
    }
}