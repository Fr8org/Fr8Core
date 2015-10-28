using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Interfaces.Manifests
{
    public class Fr8TerminalCM : Manifest
    {
        public PluginDO Definition { get; set; }

        public List<ActivityTemplateDO> Actions { get; set; }

        public Fr8TerminalCM():base(Constants.MT.Fr8Terminal){
        }
    }
}   
