using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    /// <summary>
    /// Presents a remote calendar provider such as Google, Yahoo, etc.
    /// </summary>
    public class RemoteCalendarProviderDO : BaseDO, IRemoteCalendarProviderDO
    {
        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true), MaxLength(32)]
        public string Name { get; set; }

        /// <summary>
        /// JSON string for storing Kwasant application credentials for operating with provider (for instance: ClientId, ClientSecret and Scopes for OAuth authorization)
        /// </summary>
        public string AppCreds { get; set; }

        /// <summary>
        /// Base url for accessing CalDAV API.
        /// </summary>
        public string CalDAVEndPoint { get; set; }

        [ForeignKey("AuthTypeTemplate"), Required]
        public int? AuthType { get; set; }
        public virtual _ServiceAuthorizationTypeTemplate AuthTypeTemplate { get; set; }
    }
}
