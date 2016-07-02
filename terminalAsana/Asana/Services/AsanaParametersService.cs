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
        public string ApiVersion => CloudConfigurationManager.GetSetting("AsanaApiVersion");
        public string DomainName => CloudConfigurationManager.GetSetting("AsanaApiDomainName");
        public string ApiEndpoint => this.DomainName + this.ApiVersion;

        public string Limit => CloudConfigurationManager.GetSetting("AsanaNumberOfObjectsLimit");
        public string Offset { get; }


        public string WorkspacesUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("WorlspacesUrl");
        public string TasksUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("TasksUrl");
        public string UsersUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("UsersUrl");
        public string UsersInWorkspaceUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("UsersInWorkspaceUrl");
        public string UsersMeUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("UsersMeUrl");
        public string StoriesUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("StoriesUrl");
        public string StoryUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("StoryUrl");
        public string ProjectsUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("ProjectsUrl");
        public string ProjectUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("ProjectUrl");
        public string ProjectTasksUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("ProjectTasksUrl");
        public string ProjectSectionsUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("ProjectSectionsUrl");
    }
}