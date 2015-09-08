using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.States
{
    public class AuthorizationTokenState
    {
        public const int Active = 1;
        public const int Revoked = 2;
    }
}
