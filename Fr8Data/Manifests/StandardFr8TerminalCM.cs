using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class StandardFr8TerminalCM : Manifest
    {
        public TerminalDTO Definition { get; set; }

        public List<ActivityTemplateDTO> Activities { get; set; }

        public StandardFr8TerminalCM():base(Constants.MT.StandardFr8Terminal)
        {
        }
    }
}   
