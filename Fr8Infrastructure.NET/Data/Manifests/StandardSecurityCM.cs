using fr8.Infrastructure.Data.Constants;

namespace fr8.Infrastructure.Data.Manifests
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
