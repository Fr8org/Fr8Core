using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Utilities.Configuration;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Services
{
    public class AsanaParametersService : IAsanaParameters
    {
        public string AsanaClientSecret { get; set; }
        public string AsanaClientId { get; set; }

        public string ApiVersion  { get; set; }
        public string DomainName  { get; set; }
        public string ApiEndpoint => this.DomainName + this.ApiVersion;

        public string Limit { get; set; }
        public string Offset { get; }

        //<!--OAuth section-->
        public string MinutesBeforeTokenRenewal { get; set; }
        public string AsanaOriginalRedirectUrl { get; set; }
        public string AsanaOAuthCodeUrl { get; set; }
        public string AsanaOAuthTokenUrl { get; set; }

        //<!--API URL`s-->
        public string WorkspacesUrl => this.ApiEndpoint + "/workspaces";
        public string TasksUrl => this.ApiEndpoint + "/tasks";
        public string UsersUrl => this.ApiEndpoint + "/users/{user-id}";
        public string UsersInWorkspaceUrl => this.ApiEndpoint + "/workspaces/{workspace-id}/users";
        public string UsersMeUrl => this.ApiEndpoint + "/users/me";
        public string StoriesUrl => this.ApiEndpoint + "/tasks/{task-id}/stories";
        public string StoryUrl => this.ApiEndpoint + "/stories/{story-id}";
        public string ProjectsUrl => this.ApiEndpoint + "/projects";
        public string ProjectUrl => this.ApiEndpoint + "/projects/{project-id}";
        public string ProjectTasksUrl => this.ApiEndpoint + "/projects/{project-id}/tasks";
        public string ProjectSectionsUrl => this.ApiEndpoint + "/projects/{project-id}/sections";

        public AsanaParametersService()
        {
            ApiVersion = "1.0";
            DomainName = "https://app.asana.com/api/";

            Limit = CloudConfigurationManager.GetSetting("AsanaNumberOfObjectsLimit");

            AsanaClientSecret = CloudConfigurationManager.GetSetting("AsanaClientSecret");
            AsanaClientId = CloudConfigurationManager.GetSetting("AsanaClientId");

            MinutesBeforeTokenRenewal = CloudConfigurationManager.GetSetting("MinutesBeforeTokenRenewal");
            AsanaOriginalRedirectUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") + CloudConfigurationManager.GetSetting("AsanaOriginalRedirectUrl");
            AsanaOAuthCodeUrl = CloudConfigurationManager.GetSetting("AsanaOAuthCodeUrl").Replace("%ASANA_CLIENT_ID%",AsanaClientId);
            AsanaOAuthTokenUrl = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");

        }
    }
}