using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope _transaction;
        private readonly IDBContext _context;

        internal UnitOfWork(IDBContext context)
        {
            _context = context;
            _context.UnitOfWork = this;
        }

        private AttachmentRepository _attachmentRepository;

        public AttachmentRepository AttachmentRepository
        {
            get
            {
                return _attachmentRepository ?? (_attachmentRepository = new AttachmentRepository(this));
            }
        }

        private AttendeeRepository _attendeeRepository;

        public AttendeeRepository AttendeeRepository
        {
            get
            {
                return _attendeeRepository ?? (_attendeeRepository = new AttendeeRepository(this));
            }
        }

        private EmailAddressRepository _emailAddressRepository;

        public EmailAddressRepository EmailAddressRepository
        {
            get
            {
                return _emailAddressRepository ?? (_emailAddressRepository = new EmailAddressRepository(this));
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

        private BookingRequestRepository _bookingRequestRepository;

        public BookingRequestRepository BookingRequestRepository
        {
            get
            {
                return _bookingRequestRepository ?? (_bookingRequestRepository = new BookingRequestRepository(this));
            }
        }

        private BookingRequestStatusRepository _bookingRequestStatusRepository;

        public BookingRequestStatusRepository BookingRequestStatusRepository
        {
            get
            {
                return _bookingRequestStatusRepository ?? (_bookingRequestStatusRepository = new BookingRequestStatusRepository(this));
            }
        }

        private CalendarRepository _calendarRepository;

        public CalendarRepository CalendarRepository
        {
            get
            {
                return _calendarRepository ?? (_calendarRepository = new CalendarRepository(this));
            }
        }

        private SlipRepository _SlipRepository;

        public SlipRepository SlipRepository
        {
            get
            {
                return _SlipRepository ?? (_SlipRepository = new SlipRepository(this));
            }
        }

        private RemoteCalendarProviderRepository _remoteCalendarProviderRepository;

        public RemoteCalendarProviderRepository RemoteCalendarProviderRepository
        {
            get
            {
                return _remoteCalendarProviderRepository ?? (_remoteCalendarProviderRepository = new RemoteCalendarProviderRepository(this));
            }
        }

        private RemoteCalendarAuthDataRepository _remoteCalendarAuthDataRepository;

        public RemoteCalendarAuthDataRepository RemoteCalendarAuthDataRepository
        {
            get
            {
                return _remoteCalendarAuthDataRepository ?? (_remoteCalendarAuthDataRepository = new RemoteCalendarAuthDataRepository(this));
            }
        }

        private RemoteCalendarLinkRepository _remoteCalendarLinkRepository;

        public RemoteCalendarLinkRepository RemoteCalendarLinkRepository
        {
            get
            {
                return _remoteCalendarLinkRepository ?? (_remoteCalendarLinkRepository = new RemoteCalendarLinkRepository(this));
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

        private IProcessRepository _processRepository;

        public IProcessRepository ProcessRepository
        {
            get
            {
                return _processRepository ?? (_processRepository = new ProcessRepository(this));
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

        private EnvelopeRepository _envelopeRepository;

        public EnvelopeRepository EnvelopeRepository
        {
            get
            {
                return _envelopeRepository ?? (_envelopeRepository = new EnvelopeRepository(this));
            }
        }

        private EventRepository _eventRepository;

        public EventRepository EventRepository
        {
            get
            {
                return _eventRepository ?? (_eventRepository = new EventRepository(this));
            }
        }

        private EventStatusRepository _eventStatusRepository;

        public EventStatusRepository EventStatusRepository
        {
            get
            {
                return _eventStatusRepository ?? (_eventStatusRepository = new EventStatusRepository(this));
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

        private InvitationResponseRepository _invitationResponseRepository;

        public InvitationResponseRepository InvitationResponseRepository
        {
            get
            {
                return _invitationResponseRepository ?? (_invitationResponseRepository = new InvitationResponseRepository(this));
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

        private NegotiationAnswerEmailRepository _negotiationAnswerEmailRepository;

        public NegotiationAnswerEmailRepository NegotiationAnswerEmailRepository
        {
            get
            {
                return _negotiationAnswerEmailRepository ?? (_negotiationAnswerEmailRepository = new NegotiationAnswerEmailRepository(this));
            }
        }

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

        private QuestionRepository _questionRepository;

        public QuestionRepository QuestionRepository
        {
            get
            {
                return _questionRepository ?? (_questionRepository = new QuestionRepository(this));
            }
        }

        private AnswerRepository _answerRepository;

        public AnswerRepository AnswerRepository
        {
            get
            {
                return _answerRepository ?? (_answerRepository = new AnswerRepository(this));
            }
        }

        private QuestionResponseRepository _questionResponseRepository;

        public QuestionResponseRepository QuestionResponseRepository
        {
            get
            {
                return _questionResponseRepository ?? (_questionResponseRepository = new QuestionResponseRepository(this));
            }
        }


        private NegotiationsRepository _negotiationsRepository;

        public NegotiationsRepository NegotiationsRepository
        {
            get
            {
                return _negotiationsRepository ?? (_negotiationsRepository = new NegotiationsRepository(this));
            }
        }

        private QuestionsRepository _questionsRepository;

        public QuestionsRepository QuestionsRepository
        {
            get
            {
                return _questionsRepository ?? (_questionsRepository = new QuestionsRepository(this));
            }
        }

        private AuthorizationTokenRepository _authorizationTokenRepository;

        public AuthorizationTokenRepository AuthorizationTokenRepository
        {
            get
            {
                return _authorizationTokenRepository ?? (_authorizationTokenRepository = new AuthorizationTokenRepository(this));
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

        private ProfileNodeRepository _profileNodeRepository;

        public ProfileNodeRepository ProfileNodeRepository
        {
            get
            {
                return _profileNodeRepository ?? (_profileNodeRepository = new ProfileNodeRepository(this));
            }
        }

        private ProfileItemRepository _profileItemRepository;

        public ProfileItemRepository ProfileItemRepository
        {
            get
            {
                return _profileItemRepository ?? (_profileItemRepository = new ProfileItemRepository(this));
            }
        }

        private ProfileRepository _profileRepository;

        public ProfileRepository ProfileRepository
        {
            get
            {
                return _profileRepository ?? (_profileRepository = new ProfileRepository(this));
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

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_transaction != null)
                _transaction.Dispose();
            _context.Dispose();
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
            _context.DetectChanges();
            var addedEntities = _context.AddedEntities;
            var modifiedEntities = _context.ModifiedEntities;
            var deletedEntities = _context.DeletedEntities;

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
