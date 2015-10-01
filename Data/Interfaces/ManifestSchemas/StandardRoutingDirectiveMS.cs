using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardRoutingDirectiveMS : ManifestSchema
    {
        public string Directive { get; set; }
        public string TargetProcessNodeName { get; set; }
        public string TargetActionName { get; set; }

		  public StandardRoutingDirectiveMS()
			  : base(Constants.MT.StandardRoutingDirective) { }
    }

 
}
