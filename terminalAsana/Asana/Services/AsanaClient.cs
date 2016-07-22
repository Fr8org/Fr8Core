using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Services
{
    public class AsanaClient
    {
        #region

        //------- Communication logic -------
        public IAsanaOAuth      OAuth       { get; set; }
        public IAsanaParameters Parameters  { get; set; }

        //replace it after refactoring
        //public IAsanaRestClient RestClient  { get; set; }
        protected IAsanaOAuthCommunicator  RestCommunicator { get; set; }  

        //------- Business logic -------
        public IAsanaTasks      Tasks       { get; set; }
        public IAsanaProjects   Projects    { get; set; }
        public IAsanaStories    Stories     { get; set; }
        public IAsanaUsers      Users       { get; set; }
        public IAsanaWorkspaces Workspaces  { get; set; }

        #endregion

        public AsanaClient(IAsanaParameters parameters, IRestfulServiceClient client)
        {
            Parameters = parameters;   

            OAuth = new AsanaOAuthService(client, Parameters);
            RestCommunicator = new AsanaCommunicatorService(OAuth, client);

            Tasks= new Tasks(RestCommunicator,Parameters);
            Projects = new Projects(RestCommunicator, Parameters);
            Stories = new Stories(RestCommunicator, Parameters);
            Users = new Users(RestCommunicator, Parameters);
            Workspaces = new Workspaces(RestCommunicator, Parameters);
        }

        
    }
}