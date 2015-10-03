using Data.Constants;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardAuthenticationMS : ManifestSchema
    {
        public StandardAuthenticationMS()
            : base(MT.StandardAuthentication)
        {
        }

        public AuthenticationMode Mode { get; set; }

        /// <summary>
        /// URL for external authentication.
        /// </summary>
        public string Url { get; set; }
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
        ExternalMode = 2
    }
}
