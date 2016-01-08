using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    /// <summary>
    /// This entity contains information about access to remote service providers granted by customer.
    /// The key field is AuthData that is JSON-string field with authorization data such as OAuth access and refresh token.
    /// </summary>
    public class RemoteOAuthDataDo : BaseObject, IRemoteOAuthDataDO
    {
        [NotMapped]
        IRemoteServiceProviderDO IRemoteOAuthDataDO.Provider
        {
            get { return Provider; }
            set { Provider = (RemoteServiceProviderDO)value; }
        }

        [NotMapped]
        IFr8AccountDO IRemoteOAuthDataDO.User
        {
            get { return User; }
            set { User = (Fr8AccountDO)value; }
        }

        [Key]
        public int Id { get; set; }
        public string Token { get; set; }

        [Required, ForeignKey("Provider")]
        public int? ProviderID { get; set; }
        public virtual RemoteServiceProviderDO Provider { get; set; }
        
        [Required, ForeignKey("User")]
        public string UserID { get; set; }
        public virtual Fr8AccountDO User { get; set; }        
        
        public bool HasAccessToken()
        {
            return !string.IsNullOrEmpty(Token) &&
                   Token.Contains("access_token");
        }
    }
}