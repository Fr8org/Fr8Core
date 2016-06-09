using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardSecurityCM : Manifest
    {
        public string AuthenticateAs { get; set; }

        public StandardSecurityCM()
            : base(MT.StandardSecurityCrate)
        { 
        }
    }
}
