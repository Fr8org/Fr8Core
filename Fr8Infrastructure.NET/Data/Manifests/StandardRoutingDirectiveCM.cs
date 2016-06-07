using System.Collections.Generic;
using fr8.Infrastructure.Data.Constants;

namespace fr8.Infrastructure.Data.Manifests
{
    public class StandardRoutingDirectiveCM : Manifest
    {
        public string Directive { get; set; }
        public string TargetProcessNodeName { get; set; }
        public string TargetActivityName { get; set; }
        public string Explanation { get; set; }

		  public StandardRoutingDirectiveCM()
			  : base(MT.StandardRoutingDirective) { }
    }

 
}
