using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class AmbiguityException : Exception
    {
        static string message = "There was more than one item to choose from, and I didn't know how to pick the right one.";

        public AmbiguityException() : base(message)
        {
        }
    }
}
