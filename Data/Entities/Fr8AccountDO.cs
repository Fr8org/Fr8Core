using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    public class Fr8AccountDO : IdentityUser, IFr8AccountDO, ICreateHook, ISaveHook, IModifyHook
    {
        public Fr8AccountDO()
        {
            Subscriptions = new List<SubscriptionDO>();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public Fr8AccountDO(EmailAddressDO curEmailAddress) : base()
        {
            EmailAddress = curEmailAddress;           
        }

        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Boolean TestAccount { get; set; }
        public bool SystemAccount { get; set; }
        //Booker only. Needs to be nullable otherwise DefaultValue doesn't work
        public bool? Available { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [ForeignKey("Profile")]
        public Guid? ProfileId { get; set; }
        public virtual ProfileDO Profile { get; set; }
        //This property helps differentiate visitors of the web pages in analytic tools
        //This should be either 'Internal' or 'External'
        public string Class { get; set; }

        //[NotMapped]`
        //public DocuSignAccount DocuSignAccount { get; set; }

        [Required, ForeignKey("UserStateTemplate"), DefaultValue(UserState.Active)]
        public int? State { get; set; }
        public virtual _UserStateTemplate UserStateTemplate { get; set; }

        [InverseProperty("DockyardAccount")]
        public virtual IList<SubscriptionDO> Subscriptions { get; set; }

        [ForeignKey("Organization")]
        public int? OrganizationId { get; set; }
        public virtual OrganizationDO Organization { get; set; }


        public void BeforeCreate()
        {
            if (CreateDate == default(DateTimeOffset))
                CreateDate = DateTimeOffset.UtcNow;
        }

        public void AfterCreate()
        {
        }

        public void BeforeSave()
        {
            LastUpdated = DateTimeOffset.UtcNow;
        }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            this.DetectStateUpdates(originalValues, currentValues);
        }


        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset LastUpdated { get; set; }

        public String TimeZoneID { get; set; }

        public TimeZoneInfo GetExplicitTimeZone()
        {
            if (String.IsNullOrEmpty(TimeZoneID))
                return null;

            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
        }

        [NotMapped]
        IEmailAddressDO IFr8AccountDO.EmailAddress
        {
            get { return EmailAddress; }
        }

        [NotMapped]
        IList<ISubscriptionDO> IFr8AccountDO.Subscriptions
        {
            get { return Subscriptions.Cast<ISubscriptionDO>().ToList(); }
            set { Subscriptions = value.Cast<SubscriptionDO>().ToList(); }
        }

    }
}

