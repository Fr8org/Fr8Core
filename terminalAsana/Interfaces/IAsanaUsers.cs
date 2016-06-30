using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entities;

namespace terminalAsana.Interfaces
{
    public interface IAsanaUsers
    {
        /// <summary>
        /// Returns the full user record for the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        Task<AsanaUser> Me();

        /// <summary>
        /// Returns the full user record for the single user with the provided ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AsanaUser> GetUser(int id);

        /// <summary>
        /// Returns the user records for all users in all workspaces and organizations accessible to the authenticated user. 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AsanaUser>> GetUsers(string workspaceId);


    }
}
