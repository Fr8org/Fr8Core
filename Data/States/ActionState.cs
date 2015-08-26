using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.States
{
    public class ActionState
    {
        public const int Unstarted = 1;
        public const int InProcess = 2;
        public const int Completed = 3;
        public const int Error = 4;
    }
}
