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
        Task<string> GetToken(string UserId);

        /// <summary>
        /// Get url for logging out
        /// </summary>
        /// <returns>url string</returns>
        string LogOutUrl();
    }
}
