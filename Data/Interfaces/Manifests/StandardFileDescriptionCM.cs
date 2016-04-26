using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardFileDescriptionCM : Manifest
    {
        public string DirectUrl { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public string TextRepresentation { get; set; }

		  public StandardFileDescriptionCM()
			  : base(Constants.MT.StandardFileHandle)
		  { 
		  }
    }

 
}
