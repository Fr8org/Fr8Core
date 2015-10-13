using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardRoutingDirectiveCM : Manifest
    {
        public string Directive { get; set; }
        public string TargetProcessNodeName { get; set; }
        public string TargetActionName { get; set; }

		  public StandardRoutingDirectiveCM()
			  : base(Constants.MT.StandardRoutingDirective) { }
    }

 
}
