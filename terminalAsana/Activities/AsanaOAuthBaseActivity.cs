using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public abstract class AsanaOAuthBaseActivity<T> : TerminalActivity<T>
        where T : StandardConfigurationControlsCM
    {
        protected IAsanaOAuth AsanaOAuth;
        protected IAsanaOAuthCommunicator OAuthCommunicator;

        public AsanaOAuthBaseActivity(ICrateManager crateManager, IAsanaOAuth oAuth) : base(crateManager)
        {
            AsanaOAuth = oAuth;
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
            AsanaOAuth = Task.Run(() => AsanaOAuth.InitializeAsync(this.AuthorizationToken)).Result;
            OAuthCommunicator = new AsanaCommunicatorService(AsanaOAuth, new RestfulServiceClient());
        }

    }
}