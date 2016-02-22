using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardFr8TerminalCM : Manifest
    {
        public TerminalDTO Definition { get; set; }

        public List<ActivityTemplateDTO> Activities { get; set; }

        public StandardFr8TerminalCM():base(Constants.MT.StandardFr8Terminal){
        }
    }
}   
