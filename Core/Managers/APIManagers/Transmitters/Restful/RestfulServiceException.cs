using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Managers.APIManagers.Transmitters.Restful
{
    public class RestfulServiceException : ApplicationException
    {
        public RestfulServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
