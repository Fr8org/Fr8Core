using Fr8Data.Constants;

namespace Fr8Data.Manifests
{
    public class StandardAuthenticationCM : Manifest
    {
        public StandardAuthenticationCM()
            : base(MT.StandardAuthentication)
        {
        }

        public AuthenticationMode Mode { get; set; }

        public bool Revocation { get; set; }
    }

    public enum AuthenticationMode
    {
        /// <summary>
        /// When application shows default credentials window.
        /// </summary>
        InternalMode = 1,

        /// <summary>
        /// When external auth form URL is triggered.
        /// </summary>
        ExternalMode = 2,

        // <summary>
        /// When application shows default credentials window and displays Domain textbox
        /// </summary>
        InternalModeWithDomain = 3
    }
}
