using System.Collections.Generic;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace fr8.Infrastructure.Data.Manifests
{
    public class StandardFr8TerminalCM : Manifest
    {
        public TerminalDTO Definition { get; set; }

        public List<ActivityTemplateDTO> Activities { get; set; }

        public StandardFr8TerminalCM():base(MT.StandardFr8Terminal)
        {
        }
    }
}   
