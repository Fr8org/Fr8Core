using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Interfaces.Manifests
{
    public class StandardFr8TerminalCM : Manifest
    {
        public TerminalDO Definition { get; set; }

        public List<ActivityTemplateDO> Actions { get; set; }

        public StandardFr8TerminalCM():base(Constants.MT.StandardFr8Terminal){
        }
    }
}   
