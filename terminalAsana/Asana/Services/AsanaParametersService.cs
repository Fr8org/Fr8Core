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

        public string WorkspacesUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("WorlspacesUrl");
        public string TasksUrl => this.ApiEndpoint + CloudConfigurationManager.GetSetting("TasksUrl");
    }
}