using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Transmitters.Restful
{
    public class RestfulServiceException : ApplicationException
    {
        public RestfulServiceException()
        {
            
        }

        public RestfulServiceException(string message)
            : base(message)
        {
            
        }

        public RestfulServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
