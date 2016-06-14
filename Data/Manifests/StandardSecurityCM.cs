using Fr8Data.Constants;

namespace Fr8Data.Manifests
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
