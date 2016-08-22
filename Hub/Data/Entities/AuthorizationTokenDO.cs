using Data.States.Templates;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Fr8.Infrastructure.Data.Helpers;

namespace Data.Entities
{
    public class AuthorizationTokenDO : BaseObject
    {
        public static readonly IMemberAccessor[] Members;

        static AuthorizationTokenDO()
        {
            Members = Fr8ReflectionHelper.GetMembers(typeof(AuthorizationTokenDO)).ToArray();
        }

        public AuthorizationTokenDO()
        {
            Id = Guid.NewGuid();
            
            // Do not initialize navigation properties like this.
            // It breaks entity update process.
            // When record is retrieved from DB, new instance of dynamic proxy is created derived from AuthorizationTokenDO.
            // If we manually set navigation property like this, EF will not be able to handle dynamic proxy navigation properties properly!.
            // It is ok to initialize collections, though.
            // Commented out by yakov.gnusin.
            // Terminal = new TerminalDO() { Name = "", Version = "1", TerminalStatus = TerminalStatus.Active };
        }

        public Guid Id { get; set; }

        [NotMapped]
        public String Token { get; set; }

        public String RedirectURL { get; set; }
        public String SegmentTrackingEventName { get; set; }
        public String SegmentTrackingProperties { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public String ExternalAccountId { get; set; }

        public String ExternalAccountName { get; set; }

        public String ExternalDomainId { get; set; }

        public String ExternalDomainName { get; set; }

        /// <summary>
        /// State-token parameter, that is sent to exteral auth service,
        /// and returned back when auth is completed.
        /// </summary>
        public String ExternalStateToken { get; set; }

        [ForeignKey("UserDO")]
        public String UserID { get; set; }
        // Authorization tokens are cached. We can't make lazy loading work in this case so disable it. 
        // If you need Terminal resolve it using UserRepository and UserID property.
        public Fr8AccountDO UserDO { get; set; }

        [ForeignKey("Terminal")]
        public Guid TerminalID { get; set; }
        // Authorization tokens are cached. We can't make lazy loading work in this case so disable it. 
        // If you need Terminal resolve it using ITerminal service and TerminalID property.
        public TerminalDO Terminal { get; set; }

        [ForeignKey("AuthorizationTokenStateTemplate")]
        public int? AuthorizationTokenState { get; set; }

        public virtual _AuthorizationTokenStateTemplate AuthorizationTokenStateTemplate { get; set; }

        //Additional Attributes such as version number ,  instance url etc..
        public String AdditionalAttributes { get; set; }

        public bool IsMain { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                var domain = string.IsNullOrEmpty(ExternalDomainName) ? ExternalDomainId : ExternalDomainName;
                var account = string.IsNullOrEmpty(ExternalAccountName) ? ExternalAccountId : ExternalAccountName;
                if (string.IsNullOrEmpty(domain))
                {
                    return account;
                }
                return $"{domain}\\{account}";
            }
        }

        public AuthorizationTokenDO Clone()
        {
            var clone = new AuthorizationTokenDO();

            foreach (var memberAccessor in Members.Where(x=>x.CanRead && x.CanWrite))
            {
                memberAccessor.SetValue(clone, memberAccessor.GetValue(this));
            }

            return clone;
        }
    }
}
