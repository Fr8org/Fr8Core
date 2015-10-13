using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using Utilities;
using System.Linq;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardConfigurationControlsCM : Manifest
    {
        public List<ControlDefinitionDTO> Controls { get; set; }

        public StandardConfigurationControlsCM()
			  :base(MT.StandardConfigurationControls)
        {
            Controls = new List<ControlDefinitionDTO>();
        }
		 public ControlDefinitionDTO FindByName(string name)
		  {
			  return Controls.SingleOrDefault(x => x.Name == name);
		  }
    }


}
