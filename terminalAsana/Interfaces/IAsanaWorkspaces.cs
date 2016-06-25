using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;

namespace terminalAsana.Interfaces
{
    public interface IAsanaWorkspaces
    {
        IEnumerable<AsanaWorkspace> GetAvaliableWorkspaces();

        /// <summary>
        /// Currently the only field that can be modified for a workspace is its name
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>Returns the complete, updated workspace record.</returns>
        bool UpdateWorkspace(AsanaWorkspace workspace);

        /// <summary>
        /// Returns the full workspace record for a single workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        AsanaWorkspace Get(int id);

        /// <summary>
        /// Returns the compact records for all workspaces visible to the authorized user.
        /// </summary>
        /// <returns></returns>
        IEnumerable<AsanaWorkspace> GetAll();

        /// <summary>
        /// Currently the only field that can be modified for a workspace is its name
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        AsanaWorkspace Update(AsanaWorkspace workspace);

        /// <summary>
        /// Retrieves objects in the workspace based on an auto-completion/typeahead search algorithm. This feature is meant to provide results quickly, so do not rely on this API to provide extremely accurate search results
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<string> Search(WorkspaceSearchQuery query);

        /// <summary>
        /// The user can be referenced by their globally unique user ID or their email address.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AsanaUser AddUser(AsanaUser user);

        /// <summary>
        /// The user making this call must be an admin in the workspace. Returns an empty data record.
        /// </summary>
        /// <param name="user"></param>
        bool RemoveUser(AsanaUser user);
    }
}
