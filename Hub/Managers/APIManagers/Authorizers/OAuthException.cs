using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Authorizers
{
    public class OAuthException : ApplicationException
    {
        public OAuthException()
        {
            
        }

        public OAuthException(string message)
            : base(message)
        {
            
        }

        public OAuthException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
