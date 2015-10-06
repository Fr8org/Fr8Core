using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using Utilities;
using System.Linq;

namespace Data.Interfaces.ManifestSchemas
{


    public class ManifestSchema
    {
		 public MT ManifestType { get; private set; }
		 public int ManifestId 
		 { 
			 get { return (int)ManifestType; } 
		 }
		 public string ManifestName 
		 { 
			 get { return ManifestType.GetEnumDisplayName(); }
		 }

		 public ManifestSchema(MT manifestType)
		 {
			 ManifestType = manifestType;
		 }
    }

    public class StandardConfigurationControlsMS : ManifestSchema
    {
        public List<ControlsDefinitionDTO> Controls { get; set; }

        public StandardConfigurationControlsMS()
			  :base(MT.StandardConfigurationControls)
        {
            Controls = new List<ControlsDefinitionDTO>();
        }
		 public ControlsDefinitionDTO FindByName(string name)
		  {
			  return Controls.SingleOrDefault(x => x.Name == name);
		  }
    }


}
