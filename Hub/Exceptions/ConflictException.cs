using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string typeName, string identityName, string identityValue) 
            : base($"The object of type {typeName} with '{identityName}' equal to '{identityValue}' already exists.") { }
    }
}
