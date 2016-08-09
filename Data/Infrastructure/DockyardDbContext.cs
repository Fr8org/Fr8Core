using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Data.Utility.JoinClasses;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;

namespace Data.Infrastructure
{
    [DbConfigurationType(typeof(Fr8DbConfiguration))]
    public class DockyardDbContext : IdentityDbContext<IdentityUser>, IDBContext
    {
        public static string DefaultConnectionStringName => "Fr8LocalDB";

        //This is to ensure compile will break if the reference to sql server is removed
        private static Type m_SqlProvider = typeof(SqlProviderServices);

        public class PropertyChangeInformation
        {
            public string PropertyName;
            public object OriginalValue;
            public object NewValue;

            public override string ToString() => $"[{PropertyName}]: [{OriginalValue}] -> [{NewValue}]";
        }

        public class EntityChangeInformation
        {
            public string EntityName;
            public List<PropertyChangeInformation> Changes;
        }

        public DockyardDbContext()
            : base(GetEFConnectionDetails())
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DockyardDbContext, Data.Migrations.MigrationConfiguration>());
        }

        // IMPORTANT: In order to maintain compatibility with integration tests running on Production-Staging instance, 
        // always use DockyardDbContext#GetEFConnectionDetails() to get connection details for EF 
        // and not CloudConfigurationManager directly. 
        public static string GetEFConnectionDetails()
        {
            //"Fr8.ConnectionString" is a configuration setting defined in terminalWebRole (Azure Cloud Service) 
            //and required to enable connection string management in order to run integration tests in Production/Staging 
            //environment (FR-2480).  
            var cs = CloudConfigurationManager.AppSettings.GetSetting("Fr8.ConnectionString");
            if (string.IsNullOrEmpty(cs))
            {
                return "name=" + DefaultConnectionStringName;
            }
            return cs;
        }

        public List<PropertyChangeInformation> GetEntityModifications<T>(T entity)
            where T : class
        {
            return GetEntityModifications(Entry(entity));
        }

        private List<PropertyChangeInformation> GetEntityModifications<T>(DbEntityEntry<T> entity)
            where T : class
        {
            return GetEntityModifications((DbEntityEntry)entity);
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
            var adds = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();

            var addHooks = ChangeTracker.Entries<ICreateHook>().Where(u => u.State.HasFlag(EntityState.Added)).ToList();
            var modifyHooks = ChangeTracker.Entries<IModifyHook>().Where(e => e.State == EntityState.Modified).ToList();
            var deleteHooks = ChangeTracker.Entries<IDeleteHook>().Where(e => e.State == EntityState.Deleted).ToList();
            var allHooks = ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged).ToList();

            foreach (var entity in allHooks)
            {
                entity.Entity.BeforeSave();
            }

            foreach (var entity in modifyHooks)
            {
                entity.Entity.OnModify(entity.OriginalValues, entity.CurrentValues);
            }

            foreach (var entity in deleteHooks)
            {
                entity.Entity.OnDelete(entity.OriginalValues);
            }

            //the only way we know what is being created is to look at EntityState.Added. But after the savechanges, that will all be erased.
            //so we have to build a little list of entities that will have their AfterCreate hook called.
            var createdEntityList = addHooks.ToList();

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

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
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
            modelBuilder.Entity<ContainerDO>().ToTable("Containers");
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            modelBuilder.Entity<CommunicationConfigurationDO>().ToTable("CommunicationConfigurations");
            modelBuilder.Entity<RecipientDO>().ToTable("Recipients");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<InstructionDO>().ToTable("Instructions");
            modelBuilder.Entity<InvitationDO>().ToTable("Invitations");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<TrackingStatusDO>().ToTable("TrackingStatuses");
            modelBuilder.Entity<IdentityUser>().ToTable("IdentityUsers");
            modelBuilder.Entity<UserAgentInfoDO>().ToTable("UserAgentInfos");
            modelBuilder.Entity<Fr8AccountDO>().ToTable("Users");
            modelBuilder.Entity<HistoryItemDO>().ToTable("History");
            modelBuilder.Entity<ConceptDO>().ToTable("Concepts");
            modelBuilder.Entity<SubscriptionDO>().ToTable("Subscriptions");
            modelBuilder.Entity<TerminalDO>().ToTable("Terminals");
            modelBuilder.Entity<AuthorizationTokenDO>().ToTable("AuthorizationTokens");
            modelBuilder.Entity<LogDO>().ToTable("Logs");
            modelBuilder.Entity<ExpectedResponseDO>().ToTable("ExpectedResponses");
            modelBuilder.Entity<PlanDO>().ToTable("Plans");
            modelBuilder.Entity<ActivityDO>().ToTable("Actions");
            modelBuilder.Entity<SubplanDO>().ToTable("SubPlans");
            modelBuilder.Entity<EnvelopeDO>().ToTable("Envelopes");
            modelBuilder.Entity<ActivityTemplateDO>().ToTable("ActivityTemplate");
            modelBuilder.Entity<ActivityCategoryDO>().ToTable("ActivityCategory");
            modelBuilder.Entity<ActivityCategorySetDO>().ToTable("ActivityCategorySet");
            modelBuilder.Entity<TerminalSubscriptionDO>().ToTable("TerminalSubscription");
            modelBuilder.Entity<EncryptedAuthorizationData>().ToTable("EncryptedAuthorizationData");
            modelBuilder.Entity<TagDO>().ToTable("Tags");
            modelBuilder.Entity<FileTags>().ToTable("FileTags");

            modelBuilder.Entity<PermissionSetDO>().ToTable("PermissionSets")
                .HasOptional(x=>x.Profile).WithMany(x=>x.PermissionSets).HasForeignKey(x=>x.ProfileId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PermissionSetDO>().ToTable("PermissionSets")
                .HasMany(x => x.Permissions).WithMany()
                .Map(c =>
                {
                    c.MapLeftKey("PermissionSetId");
                    c.MapRightKey("PermissionTypeTemplateId");
                    c.ToTable("PermissionSetPermissions");
                });

            modelBuilder.Entity<ProfileDO>().ToTable("Profiles")
                .HasMany(x=>x.PermissionSets).WithOptional(x=>x.Profile).HasForeignKey(x=>x.ProfileId).WillCascadeOnDelete(false);
 
            modelBuilder.Entity<FileTags>().ToTable("FileTags");

            modelBuilder.Entity<EmailDO>()
                .HasRequired(a => a.From)
                .WithMany()
                .HasForeignKey(a => a.FromID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Fr8AccountDO>()
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


            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            modelBuilder.Entity<PlanNodeDO>().ToTable("PlanNodes");

            modelBuilder.Entity<PlanNodeDO>()
                .HasOptional(x => x.ParentPlanNode)
                .WithMany(x => x.ChildNodes)
                .HasForeignKey(x => x.ParentPlanNodeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TrackingStatusDO>()
                .HasKey(ts => new
                {
                    ts.Id,
                    ts.ForeignTableName
                });

          
            modelBuilder.Entity<FileDO>().ToTable("Files");


            modelBuilder.Entity<AuthorizationTokenDO>()
             .HasRequired(x => x.Terminal)
             .WithMany()
             .HasForeignKey(x => x.TerminalID)
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<ActivityTemplateDO>()
                .HasRequired(x => x.Terminal)
                .WithMany()
                .HasForeignKey(x => x.TerminalId)

                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrganizationDO>().ToTable("Organizations")
                .HasMany(x => x.Fr8Accounts)
                .WithOptional(x => x.Organization)
                .HasForeignKey(x => x.OrganizationId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PageDefinitionDO>().ToTable("PageDefinitions");

            base.OnModelCreating(modelBuilder);
        }
    }
}