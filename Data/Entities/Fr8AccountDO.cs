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
            Profiles = new List<ProfileDO>();
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

        //Booker only. Needs to be nullable otherwise DefaultValue doesn't work
        public bool? Available { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        //it's important to persist the DocuSignAccountId. The rest of the DocuSignAccount data is accessed through the DocuSignAccount wrapper class
        public string DocusignAccountId { get; set; }

        //[NotMapped]
        //public DocuSignAccount DocuSignAccount { get; set; }

        [Required, ForeignKey("UserStateTemplate"), DefaultValue(UserState.Active)]
        public int? State { get; set; }
        public virtual _UserStateTemplate UserStateTemplate { get; set; }

        [InverseProperty("DockyardAccount")]
        public virtual IList<ProfileDO> Profiles { get; set; }

        [InverseProperty("DockyardAccount")]
        public virtual IList<SubscriptionDO> Subscriptions { get; set; }

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

        public String DisplayName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(FirstName) && !String.IsNullOrWhiteSpace(LastName))
                    return FirstName + " " + LastName;
                if (!String.IsNullOrWhiteSpace(FirstName))
                    return FirstName;
                if (!String.IsNullOrWhiteSpace(LastName))
                    return LastName;
                return UserName;
            }
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

		// TODO: The logic in this method should be changed, because we won't store offsets anymore. For more information refer to DO-1355.
        public TimeZoneInfo GetOrGuessTimeZone()
        {
            var explicitTimeZone = GetExplicitTimeZone();
            if (explicitTimeZone != null)
                return explicitTimeZone;

            var mostUsedOffset = EmailAddress.SentEmails.GroupBy(b => b.CreateDate.Offset).OrderByDescending(g => g.Count()).Select(k => (TimeSpan?)k.Key).FirstOrDefault();
            if (mostUsedOffset == null)
                return null;
            var potentialTimeZones = TimeZoneInfo.GetSystemTimeZones().Where(tzi => tzi.GetUtcOffset(DateTime.UtcNow) == mostUsedOffset.Value);
            return potentialTimeZones.FirstOrDefault();
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

