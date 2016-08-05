using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalAsana.Interfaces
{
    public interface IAsanaParameters
    {
        string ApiVersion           { get; }
        string DomainName           { get; }
        string ApiEndpoint          { get; }
        string AsanaClientSecret    { get; }
        string AsanaClientId        { get; }

        /// <summary>
        /// The number of objects to return per page. The value must be between 1 and 100.
        /// </summary>
        string Limit                { get; }

        /// <summary>
        /// Example eyJ0eXAiOJiKV1iQLCJhbGciOiJIUzI1NiJ9
        /// An offset to the next page returned by the API.A pagination request will return an offset token, which can be used as an input parameter to the next request.If an offset is not passed in, the API will return the first page of results.
        /// Note: You can only pass in an offset that was returned to you via a previously paginated request.
        /// </summary>
        string Offset               { get; }

        string MinutesBeforeTokenRenewal{ get; }
        string AsanaOriginalRedirectUrl { get; }
        string AsanaOAuthCodeUrl        { get; }
        string AsanaOAuthTokenUrl       { get; } 
                
        string WorkspacesUrl        { get; }
        string TasksUrl             { get; }
        string UsersUrl             { get; }
        string UsersInWorkspaceUrl  { get; }
        string UsersMeUrl           { get; }
        string StoriesUrl           { get; }
        string StoryUrl             { get; }
        string ProjectsUrl          { get; }
        string ProjectUrl           { get; }
        string ProjectTasksUrl      { get; }
        string ProjectSectionsUrl   { get; }
    }
}
