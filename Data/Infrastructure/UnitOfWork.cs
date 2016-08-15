using System;
 using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;
using Data.Interfaces;
using Data.Repositories;
using Data.Repositories.MultiTenant;
using Data.Repositories.Plan;
using StructureMap;

namespace Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope _transaction;
        private readonly IDBContext _context;
        private readonly IContainer _container;

        public UnitOfWork(IDBContext context, IContainer container)
        {
            _context = context;
            _context.UnitOfWork = this;
            
            // Create nested StructureMap container
            _container = container.GetNestedContainer();

            // Register self in the nested container to allow constructor injection
            _container.Configure(expression =>
            {
                expression.For<IUnitOfWork>().Use(this);
                expression.For<PlanRepository>().Use<PlanRepository>().Transient();
            });
        }

        private AttachmentRepository _attachmentRepository;

        public AttachmentRepository AttachmentRepository
        {
            get
            {
                return _attachmentRepository ?? (_attachmentRepository = new AttachmentRepository(this));
            }
        }

        //private AttendeeRepository _attendeeRepository;

        //public AttendeeRepository AttendeeRepository
        //{
        //    get
        //    {
        //        return _attendeeRepository ?? (_attendeeRepository = new AttendeeRepository(this));
        //    }
        //}

        private EmailAddressRepository _emailAddressRepository;

        public EmailAddressRepository EmailAddressRepository
        {
            get
            {
                return _emailAddressRepository ?? (_emailAddressRepository = new EmailAddressRepository(this));
            }
        }

        private EnvelopeRepository _envelopeRepository;
        public EnvelopeRepository EnvelopeRepository
        {
            get
            {
                return _envelopeRepository ?? (_envelopeRepository = new EnvelopeRepository(this));
            }
        }


        private RecipientRepository _recipientRepository;
        public RecipientRepository RecipientRepository
        {
            get
            {
                return _recipientRepository ?? (_recipientRepository = new RecipientRepository(this));
            }
        }
        
        private IProfileRepository _profileRepository;
        public IProfileRepository ProfileRepository => _profileRepository ?? (_profileRepository = new ProfileRepository(this));

        private IPermissionSetRepository _permissionSetRepository;
        public IPermissionSetRepository PermissionSetRepository => _permissionSetRepository ?? (_permissionSetRepository = new PermissionSetRepository(this));
        
        private SlipRepository _SlipRepository;

        public SlipRepository SlipRepository
        {
            get
            {
                return _SlipRepository ?? (_SlipRepository = new SlipRepository(this));
            }
        }
        
        private CommunicationConfigurationRepository _communicationConfigurationRepository;

        public CommunicationConfigurationRepository CommunicationConfigurationRepository
        {
            get
            {
                return _communicationConfigurationRepository ??
                       (_communicationConfigurationRepository = new CommunicationConfigurationRepository(this));
            }
        }

        private EmailRepository _emailRepository;

        public EmailRepository EmailRepository
        {
            get
            {
                return _emailRepository ?? (_emailRepository = new EmailRepository(this));
            }
        }

        private IContainerRepository _containerRepository;

        public IContainerRepository ContainerRepository
        {
            get
            {
                return _containerRepository ?? (_containerRepository = new ContainerRepository(this));
            }
        }
        private EmailStatusRepository _emailStatusRepository;

        public EmailStatusRepository EmailStatusRepository
        {
            get
            {
                return _emailStatusRepository ?? (_emailStatusRepository = new EmailStatusRepository(this));
            }
        }

        private InstructionRepository _instructionRepository;

        public InstructionRepository InstructionRepository
        {
            get
            {
                return _instructionRepository ?? (_instructionRepository = new InstructionRepository(this));
            }
        }

        private InvitationRepository _invitationRepository;

        public InvitationRepository InvitationRepository
        {
            get
            {
                return _invitationRepository ?? (_invitationRepository = new InvitationRepository(this));
            }
        }

    

        private StoredFileRepository _storedFileRepository;

        public StoredFileRepository StoredFileRepository
        {
            get
            {
                return _storedFileRepository ?? (_storedFileRepository = new StoredFileRepository(this));
            }
        }

        private TrackingStatusRepository _trackingStatusRepository;

        public TrackingStatusRepository TrackingStatusRepository
        {
            get
            {
                return _trackingStatusRepository ?? (_trackingStatusRepository = new TrackingStatusRepository(this));
            }
        }

        private HistoryRepository _historyRepository;

        public HistoryRepository HistoryRepository
        {
            get
            {
                return _historyRepository ?? (_historyRepository = new HistoryRepository(this));
            }
        }

        private FactRepository _factRepository;
        
        public FactRepository FactRepository
        {
            get
            {
                return _factRepository ?? (_factRepository = new FactRepository(this));
            }
        }
     
        private UserRepository _userRepository;

        public UserRepository UserRepository
        {
            get
            {
                return _userRepository ?? (_userRepository = new UserRepository(this));
            }
        }

        private UserStatusRepository _userStatusRepository;

        public UserStatusRepository UserStatusRepository
        {
            get
            {
                return _userStatusRepository ?? (_userStatusRepository = new UserStatusRepository(this));
            }
        }

        //private NegotiationAnswerEmailRepository _negotiationAnswerEmailRepository;

        //public NegotiationAnswerEmailRepository NegotiationAnswerEmailRepository
        //{
        //    get
        //    {
        //        return _negotiationAnswerEmailRepository ?? (_negotiationAnswerEmailRepository = new NegotiationAnswerEmailRepository(this));
        //    }
        //}

        private UserAgentInfoRepository _userAgentInfoRepository;

        public UserAgentInfoRepository UserAgentInfoRepository
        {
            get
            {
                return _userAgentInfoRepository ?? (_userAgentInfoRepository = new UserAgentInfoRepository(this));
            }
        }

        private AspNetUserRolesRepository _aspNetUserRolesRepository;

        public AspNetUserRolesRepository AspNetUserRolesRepository
        {
            get
            {
                return _aspNetUserRolesRepository ?? (_aspNetUserRolesRepository = new AspNetUserRolesRepository(this));
            }
        }

        private AspNetUserClaimsRepository _aspNetUserClaimsRepository;

        public IAspNetUserClaimsRepository AspNetUserClaimsRepository
        {
            get
            {
                return _aspNetUserClaimsRepository ?? (_aspNetUserClaimsRepository = new AspNetUserClaimsRepository(this));
            }
        }

        private AspNetRolesRepository _aspNetRolesRepository;

        public AspNetRolesRepository AspNetRolesRepository
        {
            get
            {
                return _aspNetRolesRepository ?? (_aspNetRolesRepository = new AspNetRolesRepository(this));
            }
        }

        private IncidentRepository _incidentRepository;

        public IncidentRepository IncidentRepository
        {
            get
            {
                return _incidentRepository ?? (_incidentRepository = new IncidentRepository(this));
            }
        }
        
        private IAuthorizationTokenRepository _authorizationTokenRepository;

        public IAuthorizationTokenRepository AuthorizationTokenRepository
        {
            get
            {
                return _authorizationTokenRepository ?? (_authorizationTokenRepository = _container.GetInstance<IAuthorizationTokenRepository>());
            }
        }

        private LogRepository _logRepository;

        public LogRepository LogRepository
        {
            get
            {
                return _logRepository ?? (_logRepository = new LogRepository(this));
            }
        }

        private ExpectedResponseRepository _expectedResponseRepository;
        public ExpectedResponseRepository ExpectedResponseRepository
        {
            get
            {
                return _expectedResponseRepository ?? (_expectedResponseRepository = new ExpectedResponseRepository(this));
            }
        }

	  private ActivityRepository _activityRepository;
	  public ActivityRepository ActivityRepository
        {
            get
            {
                return _activityRepository ?? (_activityRepository = new ActivityRepository(this));
            }
        }

        private ActivityTemplateRepository _activityTemplateRepository;
        public ActivityTemplateRepository ActivityTemplateRepository
        {
            get
            {
                return _activityTemplateRepository ?? (_activityTemplateRepository = new ActivityTemplateRepository(this));
            }
        }

	  private PlanNodeRepository _planNodeRepository;
	  public PlanNodeRepository PlanNodeRepository
	  {
		  get
		  {
			  return _planNodeRepository ?? (_planNodeRepository = new PlanNodeRepository(this));
		  }
	  }

        private SubPlanRepository _subPlanRepository;

        public ISubPlanRepository SubPlanRepository
        {
            get
            {
                return _subPlanRepository ?? (_subPlanRepository = new SubPlanRepository(this));
            }
        }


        private FileRepository _fileRepository;

        public IFileRepository FileRepository
        {
            get
            {
                return _fileRepository ?? (_fileRepository = new FileRepository(this));
            }
        }
        
        public IPlanRepository PlanRepository
        {
            get { return _container.GetInstance<PlanRepository>(); }
        }

         public IMultiTenantObjectRepository MultiTenantObjectRepository
        {
            get
            {
               return _container.GetInstance<IMultiTenantObjectRepository>();
            }
        }

        private TerminalRepository _terminalRepository;

        public ITerminalRepository TerminalRepository
        {
            get
            {
                return _terminalRepository ?? (_terminalRepository = new TerminalRepository(this));

            }
        }

        private TerminalSubscriptionRepository _terminalSubscriptionRepository;

        public ITerminalSubscriptionRepository TerminalSubscriptionRepository
        {
            get
            {
                return _terminalSubscriptionRepository ?? (_terminalSubscriptionRepository = new TerminalSubscriptionRepository(this));

            }
        }

        private SubscriptionRepository _subscriptionRepository;

        public ISubscriptionRepository SubscriptionRepository
        {
            get
            {
                return _subscriptionRepository ?? (_subscriptionRepository = new SubscriptionRepository(this));
            }
        }

        private TagRepository _tagRepository;

        public ITagRepository TagRepository
        {
            get
            {
                return _tagRepository ?? (_tagRepository = new TagRepository(this));
            }
        }

        private FileTagsRepository _fileTagsRepository;

        public IFileTagsRepository FileTagsRepository
        {
            get
            {
                return _fileTagsRepository ?? (_fileTagsRepository = new FileTagsRepository(this));
            }
        }

        private OrganizationRepository _organizationRepository;

        public IOrganizationRepository OrganizationRepository
        {
            get
            {
                return _organizationRepository ?? (_organizationRepository = new OrganizationRepository(this));
            }
        }
        
        private IPageDefinitionRepository _pageDefinitionRepository;

        public IPageDefinitionRepository PageDefinitionRepository => 
            _pageDefinitionRepository ?? (_pageDefinitionRepository = new PageDefinitionRepository(this));

        private IActivityCategoryRepository _activityCategoryRepository;
        public IActivityCategoryRepository ActivityCategoryRepository
        {
            get
            {
                return _activityCategoryRepository ?? (_activityCategoryRepository = new ActivityCategoryRepository(this));
            }
        }

        private IActivityCategorySetRepository _activityCategorySetRepository;
        public IActivityCategorySetRepository ActivityCategorySetRepository
        {
            get
            {
                return _activityCategorySetRepository ?? (_activityCategorySetRepository = new ActivityCategorySetRepository(this));
            }
        }


        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }

            _context.Dispose();
            _container.Dispose();
        }

        public void StartTransaction()
        {
            _transaction = new TransactionScope();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            SaveChanges();
            _transaction.Complete();
            _transaction.Dispose();
        }

        public void SaveChanges()
        {
            _container.GetInstance<PlanRepository>().SaveChanges();

            try
            {
                var mtRep = _container.GetInstance<IMultiTenantObjectRepository>() as MultitenantRepository;
                if (mtRep != null)
                {
                    mtRep.SaveChanges();
                }
            }
            catch
            { }

            _context.DetectChanges();
            var addedEntities = _context.AddedEntities;
            var modifiedEntities = _context.ModifiedEntities;
            var deletedEntities = _context.DeletedEntities;

            ((AuthorizationTokenRepositoryBase)AuthorizationTokenRepository).SaveChanges();

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                string errorFormat = @"Validation failed for entity [{0}]. Validation errors:" + Environment.NewLine + @"{1}";
                var errorList = new List<String>();
                foreach (var entityValidationError in e.EntityValidationErrors)
                {
                    var entityName = entityValidationError.Entry.Entity.GetType().Name;
                    var errors = String.Join(Environment.NewLine, entityValidationError.ValidationErrors.Select(a => a.PropertyName + ": " + a.ErrorMessage));
                    errorList.Add(String.Format(errorFormat, entityName, errors));
                }
                throw new Exception(String.Join(Environment.NewLine + Environment.NewLine, errorList) + Environment.NewLine, e);
            }
            
            OnEntitiesAdded(new EntitiesStateEventArgs(this, addedEntities));
            OnEntitiesModified(new EntitiesStateEventArgs(this, modifiedEntities));
            OnEntitiesDeleted(new EntitiesStateEventArgs(this, deletedEntities));
        }

        public bool IsEntityModified<TEntity>(TEntity entity) 
            where TEntity : class
        {
            return _context.Entry(entity).State == EntityState.Modified;
        }

        public IDBContext Db
        {
            get { return _context; }
        }

        /// <summary>
        /// Occurs for entities added after they saved to db.
        /// </summary>
        public static event EntitiesStateHandler EntitiesAdded;
        /// <summary>
        /// Occurs for entities modified after they saved to db.
        /// </summary>
        public static event EntitiesStateHandler EntitiesModified;
        /// <summary>
        /// Occurs for entities deleted after they removed from db.
        /// </summary>
        public static event EntitiesStateHandler EntitiesDeleted;

        private static void OnEntitiesAdded(EntitiesStateEventArgs args)
        {
            if (args.Entities == null || args.Entities.Length == 0)
                return;
            EntitiesStateHandler handler = EntitiesAdded;
            if (handler != null) handler(null, args);
        }

        private static void OnEntitiesModified(EntitiesStateEventArgs args)
        {
            if (args.Entities == null || args.Entities.Length == 0)
                return;
            EntitiesStateHandler handler = EntitiesModified;
            if (handler != null) handler(null, args);
        }

        private static void OnEntitiesDeleted(EntitiesStateEventArgs args)
        {
            if (args.Entities == null || args.Entities.Length == 0)
                return;
            EntitiesStateHandler handler = EntitiesDeleted;
            if (handler != null) handler(null, args);
        }

    }

    public delegate void EntitiesStateHandler(object sender, EntitiesStateEventArgs args);

    public class EntitiesStateEventArgs : EventArgs
    {
        public IUnitOfWork UnitOfWork { get; private set; }
        public object[] Entities { get; private set; }

        public EntitiesStateEventArgs(IUnitOfWork unitOfWork, object[] entities)
        {
            UnitOfWork = unitOfWork;
            Entities = entities;
        }
    }
}
