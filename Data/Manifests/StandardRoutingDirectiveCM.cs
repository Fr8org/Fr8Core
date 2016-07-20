using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class StandardRoutingDirectiveCM : Manifest
    {
        public string Directive { get; set; }
        public string TargetProcessNodeName { get; set; }
        public string TargetActivityName { get; set; }
        public string Explanation { get; set; }

		  public StandardRoutingDirectiveCM()
			  : base(Constants.MT.StandardRoutingDirective) { }
    }

 
}
