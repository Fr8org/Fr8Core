using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardFileHandleMS : ManifestSchema
    {
        public string DockyardStorageUrl { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }

		  public StandardFileHandleMS()
			  : base(Constants.MT.StandardFileHandle)
		  { 
		  }
    }

 
}
