namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Data.States;

    public partial class Initial_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Name = c.String(nullable: false, maxLength: 256),
                    LastUpdated = c.DateTimeOffset(precision: 7),
                    CreateDate = c.DateTimeOffset(precision: 7),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                    AspNetUserRolesDO_UserId = c.String(maxLength: 128),
                    AspNetUserRolesDO_RoleId = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUserRoles", t => new { t.AspNetUserRolesDO_UserId, t.AspNetUserRolesDO_RoleId })
                .Index(t => t.Name, unique: true, name: "RoleNameIndex")
                .Index(t => new { t.AspNetUserRolesDO_UserId, t.AspNetUserRolesDO_RoleId });

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128),
                    RoleId = c.String(nullable: false, maxLength: 128),
                    LastUpdated = c.DateTimeOffset(precision: 7),
                    CreateDate = c.DateTimeOffset(precision: 7),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);

            CreateTable(
                "dbo.AspNetUsers",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Email = c.String(maxLength: 256),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(),
                    SecurityStamp = c.String(),
                    PhoneNumber = c.String(),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                    UserName = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128),
                    ClaimType = c.String(),
                    ClaimValue = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 128),
                    ProviderKey = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Emails",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MessageID = c.String(),
                    References = c.String(),
                    Subject = c.String(),
                    HTMLText = c.String(),
                    PlainText = c.String(),
                    DateReceived = c.DateTimeOffset(nullable: false, precision: 7),
                    ConversationId = c.Int(),
                    EmailStatus = c.Int(),
                    FromID = c.Int(nullable: false),
                    FromName = c.String(),
                    ReplyToName = c.String(),
                    ReplyToAddress = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._EmailStatusTemplate", t => t.EmailStatus)
                .ForeignKey("dbo.EmailAddresses", t => t.FromID, cascadeDelete: true)
                .ForeignKey("dbo.BookingRequests", t => t.ConversationId)
                .Index(t => t.ConversationId)
                .Index(t => t.EmailStatus)
                .Index(t => t.FromID);

            CreateTable(
                "dbo.StoredFiles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OriginalName = c.String(),
                    StoredName = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._EmailStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Envelopes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Handler = c.String(),
                    TemplateName = c.String(),
                    TemplateDescription = c.String(),
                    EmailID = c.Int(nullable: false),
                    MergeData = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .Index(t => t.EmailID);

            CreateTable(
                "dbo.Events",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                    EndDate = c.DateTimeOffset(nullable: false, precision: 7),
                    Location = c.String(),
                    Transparency = c.String(),
                    Class = c.String(),
                    Description = c.String(),
                    Priority = c.Int(nullable: false),
                    Sequence = c.Int(nullable: false),
                    Summary = c.String(),
                    Category = c.String(),
                    IsAllDay = c.Boolean(nullable: false),
                    ExternalGUID = c.String(),
                    EventStatus = c.Int(),
                    CreatedByID = c.String(nullable: false, maxLength: 128),
                    CalendarID = c.Int(nullable: false),
                    BookingRequestID = c.Int(),
                    CreateType = c.Int(nullable: false),
                    SyncStatus = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Calendars", t => t.CalendarID, cascadeDelete: true)
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID)
                .ForeignKey("dbo.Users", t => t.CreatedByID)
                .ForeignKey("dbo._EventCreateTypeTemplate", t => t.CreateType, cascadeDelete: true)
                .ForeignKey("dbo._EventStatusTemplate", t => t.EventStatus)
                .ForeignKey("dbo._EventSyncStatusTemplate", t => t.SyncStatus, cascadeDelete: true)
                .Index(t => t.EventStatus)
                .Index(t => t.CreatedByID)
                .Index(t => t.CalendarID)
                .Index(t => t.BookingRequestID)
                .Index(t => t.CreateType)
                .Index(t => t.SyncStatus);

            CreateTable(
                "dbo.Attendees",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    EmailAddressID = c.Int(),
                    EventID = c.Int(),
                    NegotiationID = c.Int(),
                    ParticipationStatus = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID)
                .ForeignKey("dbo.Negotiations", t => t.NegotiationID, cascadeDelete: true)
                .ForeignKey("dbo._ParticipationStatusTemplate", t => t.ParticipationStatus)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .Index(t => t.EmailAddressID)
                .Index(t => t.EventID)
                .Index(t => t.NegotiationID)
                .Index(t => t.ParticipationStatus);

            CreateTable(
                "dbo.EmailAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Address = c.String(nullable: false, maxLength: 256),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Address, unique: true, name: "IX_EmailAddress_Address");

            CreateTable(
                "dbo.Recipients",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EmailID = c.Int(),
                    EmailAddressID = c.Int(),
                    EmailParticipantType = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._EmailParticipantTypeTemplate", t => t.EmailParticipantType)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID)
                .ForeignKey("dbo.Emails", t => t.EmailID)
                .Index(t => t.EmailID)
                .Index(t => t.EmailAddressID)
                .Index(t => t.EmailParticipantType);

            CreateTable(
                "dbo._EmailParticipantTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Negotiations",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    NegotiationState = c.Int(),
                    BookingRequestID = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._NegotiationStateTemplate", t => t.NegotiationState)
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID)
                .Index(t => t.NegotiationState)
                .Index(t => t.BookingRequestID);

            CreateTable(
                "dbo.Calendars",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    OwnerID = c.String(maxLength: 128),
                    NegotiationID = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Negotiations", t => t.NegotiationID)
                .ForeignKey("dbo.Users", t => t.OwnerID)
                .Index(t => t.OwnerID)
                .Index(t => t.NegotiationID);

            CreateTable(
                "dbo._NegotiationStateTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Questions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Text = c.String(nullable: false),
                    AnswerType = c.String(),
                    Response = c.String(),
                    QuestionStatus = c.Int(),
                    CalendarID = c.Int(),
                    NegotiationId = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Calendars", t => t.CalendarID)
                .ForeignKey("dbo._QuestionStatusTemplate", t => t.QuestionStatus)
                .ForeignKey("dbo.Negotiations", t => t.NegotiationId, cascadeDelete: true)
                .Index(t => t.QuestionStatus)
                .Index(t => t.CalendarID)
                .Index(t => t.NegotiationId);

            CreateTable(
                "dbo.Answers",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Text = c.String(),
                    QuestionID = c.Int(nullable: false),
                    EventID = c.Int(),
                    AnswerStatus = c.Int(),
                    UserID = c.String(maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AnswerStatusTemplate", t => t.AnswerStatus)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .ForeignKey("dbo.Questions", t => t.QuestionID, cascadeDelete: true)
                .Index(t => t.QuestionID)
                .Index(t => t.EventID)
                .Index(t => t.AnswerStatus)
                .Index(t => t.UserID);

            CreateTable(
                "dbo._AnswerStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._QuestionStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._ParticipationStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._EventCreateTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._EventStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._EventSyncStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._ConfirmationStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._InvitationTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._BookingRequestAvailabilityTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._BookingRequestStateTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Instructions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Category = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Profiles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    UserID = c.String(maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.ProfileNodes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    ProfileID = c.Int(nullable: false),
                    ParentNodeID = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfileNodes", t => t.ParentNodeID)
                .ForeignKey("dbo.Profiles", t => t.ProfileID, cascadeDelete: true)
                .Index(t => t.ProfileID)
                .Index(t => t.ParentNodeID);

            CreateTable(
                "dbo.ProfileItems",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProfileNodeID = c.Int(nullable: false),
                    Key = c.String(),
                    Value = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfileNodes", t => t.ProfileNodeID, cascadeDelete: true)
                .Index(t => t.ProfileNodeID);

            CreateTable(
                "dbo.RemoteCalendarAuthData",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AuthData = c.String(),
                    ProviderID = c.Int(nullable: false),
                    UserID = c.String(nullable: false, maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RemoteCalendarProviders", t => t.ProviderID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.ProviderID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.RemoteCalendarProviders",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(maxLength: 32),
                    AppCreds = c.String(),
                    CalDAVEndPoint = c.String(),
                    AuthType = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ServiceAuthorizationTypeTemplate", t => t.AuthType, cascadeDelete: true)
                .Index(t => t.Name, unique: true)
                .Index(t => t.AuthType);

            CreateTable(
                "dbo._ServiceAuthorizationTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(maxLength: 16),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._UserStateTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Slips",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CommunicationConfigurations",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CommunicationType = c.Int(),
                    ToAddress = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._CommunicationTypeTemplate", t => t.CommunicationType)
                .Index(t => t.CommunicationType);

            CreateTable(
                "dbo._CommunicationTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TrackingStatuses",
                c => new
                {
                    Id = c.Int(nullable: false),
                    ForeignTableName = c.String(nullable: false, maxLength: 128),
                    TrackingType = c.Int(),
                    TrackingStatus = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => new { t.Id, t.ForeignTableName })
                .ForeignKey("dbo._TrackingStatusTemplate", t => t.TrackingStatus)
                .ForeignKey("dbo._TrackingTypeTemplate", t => t.TrackingType)
                .Index(t => t.TrackingType)
                .Index(t => t.TrackingStatus);

            CreateTable(
                "dbo._TrackingStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo._TrackingTypeTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.UserAgentInfos",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RequestingURL = c.String(),
                    DeviceFamily = c.String(),
                    DeviceIsSpider = c.Boolean(nullable: false),
                    OSFamily = c.String(),
                    OSMajor = c.String(),
                    OSMinor = c.String(),
                    OSPatch = c.String(),
                    OSPatchMinor = c.String(),
                    AgentFamily = c.String(),
                    AgentMajor = c.String(),
                    AgentMinor = c.String(),
                    AgentPatch = c.String(),
                    UserID = c.String(maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.History",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ObjectId = c.String(),
                    BookerId = c.String(),
                    CustomerId = c.String(),
                    PrimaryCategory = c.String(),
                    SecondaryCategory = c.String(),
                    Activity = c.String(),
                    Data = c.String(),
                    Status = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    CreatedByID = c.String(maxLength: 128),
                    Priority = c.Int(),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedByID)
                .Index(t => t.CreatedByID);

            CreateTable(
                "dbo.Concepts",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    RequestId = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.RequestId)
                .Index(t => t.RequestId);

            CreateTable(
                "dbo.RemoteCalendarLinks",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RemoteCalendarHref = c.String(),
                    RemoteCalendarName = c.String(),
                    IsDisabled = c.Boolean(nullable: false),
                    LocalCalendarID = c.Int(nullable: false),
                    ProviderID = c.Int(nullable: false),
                    DateSynchronizationAttempted = c.DateTimeOffset(nullable: false, precision: 7),
                    DateSynchronized = c.DateTimeOffset(nullable: false, precision: 7),
                    LastSynchronizationResult = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Calendars", t => t.LocalCalendarID, cascadeDelete: true)
                .ForeignKey("dbo.RemoteCalendarProviders", t => t.ProviderID, cascadeDelete: true)
                .Index(t => t.LocalCalendarID)
                .Index(t => t.ProviderID);

            CreateTable(
                "dbo.QuestionResponses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AnswerID = c.Int(nullable: false),
                    UserID = c.String(nullable: false, maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Answers", t => t.AnswerID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.AnswerID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.AuthorizationTokens",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    RedirectURL = c.String(),
                    SegmentTrackingEventName = c.String(),
                    SegmentTrackingProperties = c.String(),
                    ExpiresAt = c.DateTime(nullable: false),
                    UserID = c.String(maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.Logs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Message = c.String(),
                    Name = c.String(),
                    Level = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProfileNodeAncestorsCTEView",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProfileNodeID = c.Int(nullable: false),
                    ProfileParentNodeID = c.Int(),
                    AnchorNodeID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ProfileNodeDescendantsCTEView",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProfileNodeID = c.Int(nullable: false),
                    ProfileParentNodeID = c.Int(),
                    AnchorNodeID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.NegotiationAnswerEmails",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EmailID = c.Int(nullable: false),
                    NegotiationID = c.Int(nullable: false),
                    UserID = c.String(nullable: false, maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.Negotiations", t => t.NegotiationID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.EmailID)
                .Index(t => t.NegotiationID)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.ExpectedResponses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AssociatedObjectType = c.String(),
                    AssociatedObjectID = c.Int(nullable: false),
                    UserID = c.String(maxLength: 128),
                    Status = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ExpectedResponseStatusTemplate", t => t.Status, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.Status);

            CreateTable(
                "dbo._ExpectedResponseStatusTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.EventEmail",
                c => new
                {
                    EmailID = c.Int(nullable: false),
                    EventID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.EmailID, t.EventID })
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .Index(t => t.EmailID)
                .Index(t => t.EventID);

            CreateTable(
                "dbo.BookingRequestCalendar",
                c => new
                {
                    BookingRequestID = c.Int(nullable: false),
                    CalendarID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.BookingRequestID, t.CalendarID })
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID, cascadeDelete: true)
                .ForeignKey("dbo.Calendars", t => t.CalendarID, cascadeDelete: true)
                .Index(t => t.BookingRequestID)
                .Index(t => t.CalendarID);

            CreateTable(
                "dbo.BookingRequestInstruction",
                c => new
                {
                    BookingRequestID = c.Int(nullable: false),
                    InstructionID = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.BookingRequestID, t.InstructionID })
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID, cascadeDelete: true)
                .ForeignKey("dbo.Instructions", t => t.InstructionID, cascadeDelete: true)
                .Index(t => t.BookingRequestID)
                .Index(t => t.InstructionID);

            CreateTable(
                "dbo.Attachments",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Type = c.String(),
                    ContentID = c.String(),
                    BoundaryEmbedded = c.Boolean(nullable: false),
                    EmailID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StoredFiles", t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.EmailID);

            CreateTable(
                "dbo.BookingRequests",
                c => new
                {
                    Id = c.Int(nullable: false),
                    CustomerID = c.String(nullable: false, maxLength: 128),
                    State = c.Int(nullable: false),
                    BookerID = c.String(maxLength: 128),
                    PreferredBookerID = c.String(maxLength: 128),
                    Availability = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.Users", t => t.CustomerID)
                .ForeignKey("dbo._BookingRequestStateTemplate", t => t.State, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.BookerID)
                .ForeignKey("dbo.Users", t => t.PreferredBookerID)
                .ForeignKey("dbo._BookingRequestAvailabilityTemplate", t => t.Availability)
                .Index(t => t.Id)
                .Index(t => t.CustomerID)
                .Index(t => t.State)
                .Index(t => t.BookerID)
                .Index(t => t.PreferredBookerID)
                .Index(t => t.Availability);

            CreateTable(
                "dbo.Invitations",
                c => new
                {
                    Id = c.Int(nullable: false),
                    InvitationType = c.Int(),
                    ConfirmationStatus = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo._InvitationTypeTemplate", t => t.InvitationType)
                .ForeignKey("dbo._ConfirmationStatusTemplate", t => t.ConfirmationStatus, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.InvitationType)
                .Index(t => t.ConfirmationStatus);

            CreateTable(
                "dbo.InvitationResponses",
                c => new
                {
                    Id = c.Int(nullable: false),
                    AttendeeId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.Attendees", t => t.AttendeeId)
                .Index(t => t.Id)
                .Index(t => t.AttendeeId);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    FirstName = c.String(),
                    LastName = c.String(),
                    TestAccount = c.Boolean(nullable: false),
                    Available = c.Boolean(),
                    EmailAddressID = c.Int(nullable: false),
                    State = c.Int(nullable: false),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    TimeZoneID = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID, cascadeDelete: true)
                .ForeignKey("dbo._UserStateTemplate", t => t.State, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.EmailAddressID, unique: true, name: "IX_User_EmailAddress")
                .Index(t => t.State);

            // Added by hand: CommunicationTypeRows				
            Sql(@"
SET IDENTITY_INSERT dbo._CommunicationTypeTemplate ON;
INSERT INTO dbo._CommunicationTypeTemplate (Id, Name) 
VALUES (" + CommunicationType.Email + @",'')
INSERT INTO dbo._CommunicationTypeTemplate (Id, Name) 
VALUES (" + CommunicationType.Sms + @",'')
SET IDENTITY_INSERT dbo._CommunicationTypeTemplate OFF;");

            DropForeignKey("dbo.CommunicationConfigurations", "CommunicationType", "dbo._CommunicationTypeTemplate");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Users", "State", "dbo._UserStateTemplate");
            DropForeignKey("dbo.Users", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Users", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.InvitationResponses", "AttendeeId", "dbo.Attendees");
            DropForeignKey("dbo.InvitationResponses", "Id", "dbo.Emails");
            DropForeignKey("dbo.Invitations", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            DropForeignKey("dbo.Invitations", "InvitationType", "dbo._InvitationTypeTemplate");
            DropForeignKey("dbo.Invitations", "Id", "dbo.Emails");
            DropForeignKey("dbo.BookingRequests", "Availability", "dbo._BookingRequestAvailabilityTemplate");
            DropForeignKey("dbo.BookingRequests", "PreferredBookerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "BookerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "State", "dbo._BookingRequestStateTemplate");
            DropForeignKey("dbo.BookingRequests", "CustomerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "Id", "dbo.StoredFiles");
            DropForeignKey("dbo.ExpectedResponses", "UserID", "dbo.Users");
            DropForeignKey("dbo.ExpectedResponses", "Status", "dbo._ExpectedResponseStatusTemplate");
            DropForeignKey("dbo.NegotiationAnswerEmails", "UserID", "dbo.Users");
            DropForeignKey("dbo.NegotiationAnswerEmails", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.NegotiationAnswerEmails", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.AuthorizationTokens", "UserID", "dbo.Users");
            DropForeignKey("dbo.QuestionResponses", "UserID", "dbo.Users");
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropForeignKey("dbo.RemoteCalendarLinks", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarLinks", "LocalCalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Concepts", "RequestId", "dbo.Emails");
            DropForeignKey("dbo.History", "CreatedByID", "dbo.Users");
            DropForeignKey("dbo.UserAgentInfos", "UserID", "dbo.Users");
            DropForeignKey("dbo.TrackingStatuses", "TrackingType", "dbo._TrackingTypeTemplate");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatus", "dbo._TrackingStatusTemplate");
            DropForeignKey("dbo.CommunicationConfigurations", "CommunicationType", "dbo._CommunicationTypeTemplate");
            DropForeignKey("dbo.RemoteCalendarAuthData", "UserID", "dbo.Users");
            DropForeignKey("dbo.RemoteCalendarAuthData", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthType", "dbo._ServiceAuthorizationTypeTemplate");
            DropForeignKey("dbo.Profiles", "UserID", "dbo.Users");
            DropForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles");
            DropForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.Calendars", "OwnerID", "dbo.Users");
            DropForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestInstruction", "InstructionID", "dbo.Instructions");
            DropForeignKey("dbo.BookingRequestInstruction", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Emails", "ConversationId", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestCalendar", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.BookingRequestCalendar", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.EventEmail", "EventID", "dbo.Events");
            DropForeignKey("dbo.EventEmail", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Events", "SyncStatus", "dbo._EventSyncStatusTemplate");
            DropForeignKey("dbo.Events", "EventStatus", "dbo._EventStatusTemplate");
            DropForeignKey("dbo.Events", "CreateType", "dbo._EventCreateTypeTemplate");
            DropForeignKey("dbo.Events", "CreatedByID", "dbo.Users");
            DropForeignKey("dbo.Events", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations");
            DropForeignKey("dbo.Questions", "QuestionStatus", "dbo._QuestionStatusTemplate");
            DropForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Answers", "UserID", "dbo.Users");
            DropForeignKey("dbo.Answers", "EventID", "dbo.Events");
            DropForeignKey("dbo.Answers", "AnswerStatus", "dbo._AnswerStatusTemplate");
            DropForeignKey("dbo.Negotiations", "NegotiationState", "dbo._NegotiationStateTemplate");
            DropForeignKey("dbo.Calendars", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.Events", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Emails", "FromID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailParticipantType", "dbo._EmailParticipantTypeTemplate");
            DropForeignKey("dbo.Envelopes", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Emails", "EmailStatus", "dbo._EmailStatusTemplate");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" }, "dbo.AspNetUserRoles");
            DropIndex("dbo.Users", new[] { "State" });
            DropIndex("dbo.Users", "IX_User_EmailAddress");
            DropIndex("dbo.Users", new[] { "Id" });
            DropIndex("dbo.InvitationResponses", new[] { "AttendeeId" });
            DropIndex("dbo.InvitationResponses", new[] { "Id" });
            DropIndex("dbo.Invitations", new[] { "ConfirmationStatus" });
            DropIndex("dbo.Invitations", new[] { "InvitationType" });
            DropIndex("dbo.Invitations", new[] { "Id" });
            DropIndex("dbo.BookingRequests", new[] { "Availability" });
            DropIndex("dbo.BookingRequests", new[] { "PreferredBookerID" });
            DropIndex("dbo.BookingRequests", new[] { "BookerID" });
            DropIndex("dbo.BookingRequests", new[] { "State" });
            DropIndex("dbo.BookingRequests", new[] { "CustomerID" });
            DropIndex("dbo.BookingRequests", new[] { "Id" });
            DropIndex("dbo.Attachments", new[] { "EmailID" });
            DropIndex("dbo.Attachments", new[] { "Id" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "InstructionID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "BookingRequestID" });
            DropIndex("dbo.BookingRequestCalendar", new[] { "CalendarID" });
            DropIndex("dbo.BookingRequestCalendar", new[] { "BookingRequestID" });
            DropIndex("dbo.EventEmail", new[] { "EventID" });
            DropIndex("dbo.EventEmail", new[] { "EmailID" });
            DropIndex("dbo.ExpectedResponses", new[] { "Status" });
            DropIndex("dbo.ExpectedResponses", new[] { "UserID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "UserID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "NegotiationID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "EmailID" });
            DropIndex("dbo.AuthorizationTokens", new[] { "UserID" });
            DropIndex("dbo.QuestionResponses", new[] { "UserID" });
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            DropIndex("dbo.RemoteCalendarLinks", new[] { "ProviderID" });
            DropIndex("dbo.RemoteCalendarLinks", new[] { "LocalCalendarID" });
            DropIndex("dbo.Concepts", new[] { "RequestId" });
            DropIndex("dbo.History", new[] { "CreatedByID" });
            DropIndex("dbo.UserAgentInfos", new[] { "UserID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingStatus" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingType" });
            DropIndex("dbo.CommunicationConfigurations", new[] { "CommunicationType" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "AuthType" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "Name" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "UserID" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "ProviderID" });
            DropIndex("dbo.ProfileItems", new[] { "ProfileNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ParentNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ProfileID" });
            DropIndex("dbo.Profiles", new[] { "UserID" });
            DropIndex("dbo.Answers", new[] { "UserID" });
            DropIndex("dbo.Answers", new[] { "AnswerStatus" });
            DropIndex("dbo.Answers", new[] { "EventID" });
            DropIndex("dbo.Answers", new[] { "QuestionID" });
            DropIndex("dbo.Questions", new[] { "NegotiationId" });
            DropIndex("dbo.Questions", new[] { "CalendarID" });
            DropIndex("dbo.Questions", new[] { "QuestionStatus" });
            DropIndex("dbo.Calendars", new[] { "NegotiationID" });
            DropIndex("dbo.Calendars", new[] { "OwnerID" });
            DropIndex("dbo.Negotiations", new[] { "BookingRequestID" });
            DropIndex("dbo.Negotiations", new[] { "NegotiationState" });
            DropIndex("dbo.Recipients", new[] { "EmailParticipantType" });
            DropIndex("dbo.Recipients", new[] { "EmailAddressID" });
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            DropIndex("dbo.EmailAddresses", "IX_EmailAddress_Address");
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            DropIndex("dbo.Attendees", new[] { "NegotiationID" });
            DropIndex("dbo.Attendees", new[] { "EventID" });
            DropIndex("dbo.Attendees", new[] { "EmailAddressID" });
            DropIndex("dbo.Events", new[] { "SyncStatus" });
            DropIndex("dbo.Events", new[] { "CreateType" });
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            DropIndex("dbo.Events", new[] { "CalendarID" });
            DropIndex("dbo.Events", new[] { "CreatedByID" });
            DropIndex("dbo.Events", new[] { "EventStatus" });
            DropIndex("dbo.Envelopes", new[] { "EmailID" });
            DropIndex("dbo.Emails", new[] { "FromID" });
            DropIndex("dbo.Emails", new[] { "EmailStatus" });
            DropIndex("dbo.Emails", new[] { "ConversationId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Users");
            DropTable("dbo.InvitationResponses");
            DropTable("dbo.Invitations");
            DropTable("dbo.BookingRequests");
            DropTable("dbo.Attachments");
            DropTable("dbo.BookingRequestInstruction");
            DropTable("dbo.BookingRequestCalendar");
            DropTable("dbo.EventEmail");
            DropTable("dbo._ExpectedResponseStatusTemplate");
            DropTable("dbo.ExpectedResponses");
            DropTable("dbo.NegotiationAnswerEmails");
            DropTable("dbo.ProfileNodeDescendantsCTEView");
            DropTable("dbo.ProfileNodeAncestorsCTEView");
            DropTable("dbo.Logs");
            DropTable("dbo.AuthorizationTokens");
            DropTable("dbo.QuestionResponses");
            DropTable("dbo.RemoteCalendarLinks");
            DropTable("dbo.Concepts");
            DropTable("dbo.History");
            DropTable("dbo.UserAgentInfos");
            DropTable("dbo._TrackingTypeTemplate");
            DropTable("dbo._TrackingStatusTemplate");
            DropTable("dbo.TrackingStatuses");
            DropTable("dbo._CommunicationTypeTemplate");
            DropTable("dbo.CommunicationConfigurations");
            DropTable("dbo.Slips");
            DropTable("dbo._UserStateTemplate");
            DropTable("dbo._ServiceAuthorizationTypeTemplate");
            DropTable("dbo.RemoteCalendarProviders");
            DropTable("dbo.RemoteCalendarAuthData");
            DropTable("dbo.ProfileItems");
            DropTable("dbo.ProfileNodes");
            DropTable("dbo.Profiles");
            DropTable("dbo.Instructions");
            DropTable("dbo._BookingRequestStateTemplate");
            DropTable("dbo._BookingRequestAvailabilityTemplate");
            DropTable("dbo._InvitationTypeTemplate");
            DropTable("dbo._ConfirmationStatusTemplate");
            DropTable("dbo._EventSyncStatusTemplate");
            DropTable("dbo._EventStatusTemplate");
            DropTable("dbo._EventCreateTypeTemplate");
            DropTable("dbo._ParticipationStatusTemplate");
            DropTable("dbo._QuestionStatusTemplate");
            DropTable("dbo._AnswerStatusTemplate");
            DropTable("dbo.Answers");
            DropTable("dbo.Questions");
            DropTable("dbo._NegotiationStateTemplate");
            DropTable("dbo.Calendars");
            DropTable("dbo.Negotiations");
            DropTable("dbo._EmailParticipantTypeTemplate");
            DropTable("dbo.Recipients");
            DropTable("dbo.EmailAddresses");
            DropTable("dbo.Attendees");
            DropTable("dbo.Events");
            DropTable("dbo.Envelopes");
            DropTable("dbo._EmailStatusTemplate");
            DropTable("dbo.StoredFiles");
            DropTable("dbo.Emails");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}
