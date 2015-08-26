using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection;
using Data.Entities.CTE;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace Data.Infrastructure
{
    public class DockyardDbContext : IdentityDbContext<IdentityUser>, IDBContext
    {
        //This is to ensure compile will break if the reference to sql server is removed
        private static Type m_SqlProvider = typeof(SqlProviderServices);

        public class PropertyChangeInformation
        {
            public String PropertyName;
            public Object OriginalValue;
            public Object NewValue;

            public override string ToString()
            {
                
                const string displayChange = "[{0}]: [{1}] -> [{2}]";
                return String.Format(displayChange, PropertyName, OriginalValue, NewValue);
            }
        }

        public class EntityChangeInformation
        {
            public String EntityName;
            public List<PropertyChangeInformation> Changes;
        }

        //Do not change this value! If you want to change the database you connect to, edit your web.config file
        public DockyardDbContext()
            : base("name=DockyardDB")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DockyardDbContext, Data.Migrations.MigrationConfiguration>()); 
        }


        public List<PropertyChangeInformation> GetEntityModifications<T>(T entity)
            where T : class
        {
            return GetEntityModifications(Entry(entity));
        }

        private List<PropertyChangeInformation> GetEntityModifications<T>(DbEntityEntry<T> entity)
            where T : class
        {
            return GetEntityModifications((DbEntityEntry) entity);
        }

        public void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

        public object[] AddedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToArray(); }
        }

        public object[] ModifiedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToArray(); }
        }

        public object[] DeletedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToArray(); }
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            //Debug code!
            List<object> adds = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            List<object> modifies = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToList();
            List<object> deletes = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            List<object> all = ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).Select(e => e.Entity).ToList();

            List<DbEntityEntry<ICreateHook>> addHooks = ChangeTracker.Entries<ICreateHook>().Where(u => u.State.HasFlag(EntityState.Added)).ToList();
            List<DbEntityEntry<IModifyHook>> modifyHooks = ChangeTracker.Entries<IModifyHook>().Where(e => e.State == EntityState.Modified).ToList();
            List<DbEntityEntry<IDeleteHook>> deleteHooks = ChangeTracker.Entries<IDeleteHook>().Where(e => e.State == EntityState.Deleted).ToList();
            List<DbEntityEntry<ISaveHook>> allHooks = ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged).ToList();

            var uow = new UnitOfWork(this);

            foreach (DbEntityEntry<ISaveHook> entity in allHooks)
            {
                entity.Entity.BeforeSave();
            }

            foreach (DbEntityEntry<IModifyHook> entity in modifyHooks)
            {
                entity.Entity.OnModify(entity.OriginalValues, entity.CurrentValues);
            }

            foreach (DbEntityEntry<IDeleteHook> entity in deleteHooks)
            {
                entity.Entity.OnDelete(entity.OriginalValues);
            }

            //the only way we know what is being created is to look at EntityState.Added. But after the savechanges, that will all be erased.
            //so we have to build a little list of entities that will have their AfterCreate hook called.
            var createdEntityList = new List<DbEntityEntry<ICreateHook>>();
            foreach (DbEntityEntry<ICreateHook> entity in addHooks)
            {
               createdEntityList.Add(entity);
            }

            FixForeignKeyIDs(adds);

            foreach (var createdEntity in createdEntityList)
            {
                createdEntity.Entity.BeforeCreate();
            }

            var saveResult = base.SaveChanges();

            foreach (var createdEntity in createdEntityList)
            {
                createdEntity.Entity.AfterCreate();
            }
            
            return saveResult;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// This method will take all 'new' rows, and assign them foreign IDs _if_ they have set a foreign row.
        /// This fixes an issue with EF, so we can do this:
        /// attachment.Email = emailDO
        /// 
        /// instead of this:
        /// 
        /// attachment.Email = emailDO;
        /// attachment.EmailID = emailDO.Id;
        /// 
        /// We look at the attributes on the properties of our entities, and figure out which rows require updating
        /// </summary>
        private void FixForeignKeyIDs(IEnumerable<object> adds)
        {
            foreach (var grouping in adds.GroupBy(r => r.GetType()))
            {
                if (!grouping.Any())
                    continue;

                //First, we check if the entity has foreign relationships
                var propType = grouping.Key;
                var props = propType.GetProperties();
                var propsWithForeignKeyNotation = props.Where(p => p.GetCustomAttribute<ForeignKeyAttribute>(true) != null).ToList();
                if (!propsWithForeignKeyNotation.Any())
                    continue;

                //Then we loop through each relationship
                foreach (var prop in propsWithForeignKeyNotation)
                {
                    var attr = prop.GetCustomAttribute<ForeignKeyAttribute>(true);
                    
                    //Now.. find out which way it goes..
                    var linkedName = attr.Name;
                    var linkedProp = propType.GetProperties().FirstOrDefault(n => n.Name == linkedName);
                    if (linkedProp == null)
                        continue;

                    PropertyInfo foreignIDProperty;
                    PropertyInfo parentFKIDProperty;
                    PropertyInfo parentFKDOProperty;

                    var linkedID = ReflectionHelper.EntityPrimaryKeyPropertyInfo(linkedProp.PropertyType);

                    //If linkedID != null, it means we defined the attribute on the KEY property, rather than the row property
                    //Ie, we defined something like this:

                    //[ForeignKey("Email")]
                    //int EmailID {get;set;}
                    //EmailDO Email {get;set;}
                    if (linkedID != null)
                    {
                        foreignIDProperty = linkedID;
                        parentFKIDProperty = prop;
                        parentFKDOProperty = linkedProp;
                    }

                    //If linkedID == null, it means we defined the attribute on the ROW property, rather than the key property
                    //Ie, we defined something like this:

                    //int EmailID {get;set;}
                    //[ForeignKey("EmailID")]
                    //EmailDO Email {get;set;}
                    else
                    {
                        foreignIDProperty = ReflectionHelper.EntityPrimaryKeyPropertyInfo(prop.PropertyType);
                        parentFKIDProperty = linkedProp;
                        parentFKDOProperty = prop;
                    }

                    //Something bad happened - it means we defined the keys using fluent code-to-sql
                    //In this case, there's nothing we can do.. ignore this attempt
                    if (foreignIDProperty == null)
                        continue;

                    foreach (var value in grouping)
                    {
                        //Find the foreign row
                        var foreignDO = parentFKDOProperty.GetValue(value);
                        if (foreignDO != null) //If the DO is set, then we update the ID
                        {
                            var fkID = foreignIDProperty.GetValue(foreignDO);
                            parentFKIDProperty.SetValue(value, fkID);
                        }
                    }
                }
            }
        }


        public IUnitOfWork UnitOfWork { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessDO>().ToTable("Processes");
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            //modelBuilder.Entity<AttendeeDO>().ToTable("Attendees");
            //modelBuilder.Entity<BookingRequestDO>().ToTable("BookingRequests");
            //modelBuilder.Entity<CalendarDO>().ToTable("Calendars");
            //modelBuilder.Entity<QuestionDO>().ToTable("Questions");
            modelBuilder.Entity<CommunicationConfigurationDO>().ToTable("CommunicationConfigurations");
            modelBuilder.Entity<RecipientDO>().ToTable("Recipients");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EnvelopeDO>().ToTable("Envelopes");
            //modelBuilder.Entity<EventDO>().ToTable("Events");
            modelBuilder.Entity<InstructionDO>().ToTable("Instructions");
            modelBuilder.Entity<InvitationDO>().ToTable("Invitations");
            //modelBuilder.Entity<InvitationResponseDO>().ToTable("InvitationResponses");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<TrackingStatusDO>().ToTable("TrackingStatuses");
            modelBuilder.Entity<IdentityUser>().ToTable("IdentityUsers");
            modelBuilder.Entity<UserAgentInfoDO>().ToTable("UserAgentInfos");
            modelBuilder.Entity<DockyardAccountDO>().ToTable("Users");
            modelBuilder.Entity<HistoryItemDO>().ToTable("History");
            modelBuilder.Entity<ConceptDO>().ToTable("Concepts");
            modelBuilder.Entity<SubscriptionDO>().ToTable("Subscriptions");
            modelBuilder.Entity<PluginDO>().ToTable("Plugins");
            //modelBuilder.Entity<NegotiationDO>().ToTable("Negotiations");
            //modelBuilder.Entity<AnswerDO>().ToTable("Answers");
            modelBuilder.Entity<RemoteCalendarProviderDO>().ToTable("RemoteCalendarProviders");
            modelBuilder.Entity<RemoteCalendarAuthDataDO>().ToTable("RemoteCalendarAuthData");
            //modelBuilder.Entity<RemoteCalendarLinkDO>().ToTable("RemoteCalendarLinks");
            //modelBuilder.Entity<QuestionResponseDO>().ToTable("QuestionResponses");
            modelBuilder.Entity<AuthorizationTokenDO>().ToTable("AuthorizationTokens");
            modelBuilder.Entity<LogDO>().ToTable("Logs");
            modelBuilder.Entity<ProfileDO>().ToTable("Profiles");
            modelBuilder.Entity<ProfileNodeDO>().ToTable("ProfileNodes");
            modelBuilder.Entity<ProfileItemDO>().ToTable("ProfileItems");
            modelBuilder.Entity<ProfileNodeAncestorsCTE>().ToTable("ProfileNodeAncestorsCTEView");
            modelBuilder.Entity<ProfileNodeDescendantsCTE>().ToTable("ProfileNodeDescendantsCTEView");
            //modelBuilder.Entity<NegotiationAnswerEmailDO>().ToTable("NegotiationAnswerEmails");
            modelBuilder.Entity<ExpectedResponseDO>().ToTable("ExpectedResponses");
            modelBuilder.Entity<ProcessTemplateDO>().ToTable("ProcessTemplates");
            modelBuilder.Entity<ActionDO>().ToTable("Actions");
            modelBuilder.Entity<ActionListDO>().ToTable("ActionLists");
            modelBuilder.Entity<TemplateDO>().ToTable("Templates");
            modelBuilder.Entity<ProcessNodeDO>().ToTable("ProcessNodes");
            modelBuilder.Entity<ProcessNodeTemplateDO>().ToTable("ProcessNodeTemplates");
            modelBuilder.Entity<ExternalEventSubscriptionDO>().ToTable("ExternalEventRegistrations");
            modelBuilder.Entity<DocuSignEventDO>().ToTable("DocuSignEvents");
            modelBuilder.Entity<MailerDO>().ToTable("Mailers");
            modelBuilder.Entity<ActionRegistrationDO>().ToTable("ActionRegistration");
            modelBuilder.Entity<DocuSignTemplateSubscriptionDO>().ToTable("DocuSignTemplateSubscriptions");

            modelBuilder.Entity<EmailDO>()
                .HasRequired(a => a.From)
                .WithMany()
                .HasForeignKey(a => a.FromID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProcessNodeDO>()
                .HasRequired<ProcessDO>(pn => pn.ParentProcess)
                .WithMany(p => p.ProcessNodes)
                .HasForeignKey(pn => pn.ParentProcessId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DockyardAccountDO>()
                .Property(u => u.EmailAddressID)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_User_EmailAddress", 1) { IsUnique = true }));

            modelBuilder.Entity<EmailAddressDO>()
                .Property(ea => ea.Address)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_EmailAddress_Address", 1) { IsUnique = true }));

            //modelBuilder.Entity<EventDO>()
            //    .HasMany(ev => ev.Emails)
            //    .WithMany(e => e.Events)
            //    .Map(
            //        mapping => mapping.MapLeftKey("EventID").MapRightKey("EmailID").ToTable("EventEmail")
            //    );

            //modelBuilder.Entity<EventDO>()
            //    .HasMany(ev => ev.Attendees)
            //    .WithOptional(a => a.Event)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<CalendarDO>()
            //    .HasMany(ev => ev.BookingRequests)
            //    .WithMany(e => e.Calendars)
            //    .Map(
            //        mapping => mapping.MapLeftKey("CalendarID").MapRightKey("BookingRequestID").ToTable("BookingRequestCalendar")
            //    );

            //modelBuilder.Entity<BookingRequestDO>()
            //    .HasMany(ev => ev.Instructions)
            //    .WithMany()
            //    .Map(
            //        mapping => mapping.MapLeftKey("BookingRequestID").MapRightKey("InstructionID").ToTable("BookingRequestInstruction")
            //    );

         
            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            //modelBuilder.Entity<NegotiationDO>()
            //    .HasMany(e => e.Questions)
            //    .WithRequired(a => a.Negotiation)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<NegotiationDO>()
            //    .HasMany(e => e.Attendees)
            //    .WithOptional(a => a.Negotiation)
            //    .WillCascadeOnDelete(true);
            
            modelBuilder.Entity<TrackingStatusDO>()
                .HasKey(ts => new
                {
                    ts.Id,
                    ts.ForeignTableName
                });
            
            //modelBuilder.Entity<AnswerDO>()
            //    .HasOptional(a => a.Event).WithMany().WillCascadeOnDelete();

            //modelBuilder.Entity<QuestionDO>()
            //    .HasMany(e => e.Answers)
            //    .WithRequired(a => a.Question)
            //    .WillCascadeOnDelete(true);

            modelBuilder.Entity<CriteriaDO>().ToTable("Criteria");
            modelBuilder.Entity<FileDO>().ToTable("Files");

            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<Data.Entities.ProcessDO> Processes { get; set; }
    }
}