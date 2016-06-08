using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardFileDescriptionCM : Manifest
    {
        public string DirectUrl { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public string TextRepresentation { get; set; }

		  public StandardFileDescriptionCM()
			  : base(MT.StandardFileHandle)
		  { 
		  }
    }

 
}
