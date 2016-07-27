using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IPlanDirectory
    {
        /// <summary>
        /// Get token for user authentication from Plan Directory 
        /// </summary>
        /// <param name="UserId">User who will be authenticated in PD</param>
        /// <returns></returns>
        Task<string> GetAuthToken(string UserId);

        /// <summary>
        /// Logiout user from PD in case he was uthenticated
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<bool> Logout(string UserId);
    }
}
