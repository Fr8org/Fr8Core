using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entites;

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
    }
}
