using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalAsana.Asana
{
    public class WorkspaceSearchQuery
    {
        /// <summary>
        /// The workspace to fetch objects from.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Supported values: user, task, workspace
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The string that will be used to search for relevant objects. If an empty string is passed in, the API will currently return an empty result set.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The number of results to return. The default is 20 if this parameter is omitted, with a minimum of 1 and a maximum of 100. If there are fewer results found than requested, all will be returned.
        /// </summary>
        public short Count { get; set; }
    }
}