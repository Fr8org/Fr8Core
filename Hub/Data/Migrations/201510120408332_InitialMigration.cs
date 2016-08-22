namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : System.Data.Entity.Migrations.DbMigration
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
                .ForeignKey("dbo.Emails", t => t.EmailID)
                .ForeignKey("dbo._EmailParticipantTypeTemplate", t => t.EmailParticipantType)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID)
                .Index(t => t.EmailID)
                .Index(t => t.EmailAddressID)
                .Index(t => t.EmailParticipantType);
            
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
                "dbo._EmailParticipantTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DockyardAccountID = c.String(maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.DockyardAccountID)
                .Index(t => t.DockyardAccountID);
            
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
                "dbo.Subscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DockyardAccountId = c.String(maxLength: 128),
                        PluginId = c.Int(nullable: false),
                        AccessLevel = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AccessLevelTemplate", t => t.AccessLevel, cascadeDelete: true)
                .ForeignKey("dbo.Plugins", t => t.PluginId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.DockyardAccountId)
                .Index(t => t.DockyardAccountId)
                .Index(t => t.PluginId)
                .Index(t => t.AccessLevel);
            
            CreateTable(
                "dbo._AccessLevelTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Plugins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Version = c.String(nullable: false),
                        PluginStatus = c.Int(nullable: false),
                        RequiresAuthentication = c.Boolean(nullable: false),
                        Endpoint = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._PluginStatusTemplate", t => t.PluginStatus, cascadeDelete: true)
                .Index(t => t.PluginStatus);
            
            CreateTable(
                "dbo._PluginStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
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
                "dbo.Processes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DockyardAccountId = c.String(),
                        ProcessTemplateId = c.Int(nullable: false),
                        ProcessState = c.Int(nullable: false),
                        CurrentActivityId = c.Int(),
                        NextActivityId = c.Int(),
                        CrateStorage = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessTemplates", t => t.ProcessTemplateId)
                // .ForeignKey("dbo.ActivityDOes", t => t.CurrentActivityId)
                // .ForeignKey("dbo.ActivityDOes", t => t.NextActivityId)
                .ForeignKey("dbo._ProcessStateTemplate", t => t.ProcessState, cascadeDelete: true)
                .Index(t => t.ProcessTemplateId)
                .Index(t => t.ProcessState)
                .Index(t => t.CurrentActivityId)
                .Index(t => t.NextActivityId);
            
            CreateTable(
                "dbo.ActivityDOes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentActivityId = c.Int(),
                        Ordering = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.ParentActivityId)
                .Index(t => t.ParentActivityId);
            
            CreateTable(
                "dbo._ActionStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ActivityTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Version = c.String(),
                        AuthenticationType = c.String(),
                        ComponentActivities = c.String(),
                        PluginID = c.Int(nullable: false),
                        Category = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Plugins", t => t.PluginID)
                .Index(t => t.PluginID);
            
            CreateTable(
                "dbo.Criteria",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProcessNodeTemplateId = c.Int(),
                        CriteriaExecutionType = c.Int(nullable: false),
                        ConditionsJSON = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._CriteriaExecutionTypeTemplate", t => t.CriteriaExecutionType, cascadeDelete: true)
                .ForeignKey("dbo.ProcessNodeTemplates", t => t.ProcessNodeTemplateId)
                .Index(t => t.ProcessNodeTemplateId)
                .Index(t => t.CriteriaExecutionType);
            
            CreateTable(
                "dbo._CriteriaExecutionTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProcessNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ParentProcessId = c.Int(nullable: false),
                        ProcessNodeState = c.Int(),
                        ProcessNodeTemplateId = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Processes", t => t.ParentProcessId)
                .ForeignKey("dbo._ProcessNodeStatusTemplate", t => t.ProcessNodeState)
                .ForeignKey("dbo.ProcessNodeTemplates", t => t.ProcessNodeTemplateId)
                .Index(t => t.ParentProcessId)
                .Index(t => t.ProcessNodeState)
                .Index(t => t.ProcessNodeTemplateId);
            
            CreateTable(
                "dbo._ProcessNodeStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ProcessTemplateStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ProcessStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
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
                "dbo.RemoteCalendarProviders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 32),
                        AppCreds = c.String(),
                        EndPoint = c.String(),
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
                "dbo.RemoteCalendarAuthData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Token = c.String(),
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
                "dbo.AuthorizationTokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Token = c.String(),
                        RedirectURL = c.String(),
                        SegmentTrackingEventName = c.String(),
                        SegmentTrackingProperties = c.String(),
                        ExpiresAt = c.DateTime(nullable: false),
                        ExternalAccountId = c.String(),
                        ExternalStateToken = c.String(),
                        UserID = c.String(maxLength: 128),
                        PluginID = c.Int(nullable: false),
                        AuthorizationTokenState = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AuthorizationTokenStateTemplate", t => t.AuthorizationTokenState)
                .ForeignKey("dbo.Plugins", t => t.PluginID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.PluginID)
                .Index(t => t.AuthorizationTokenState);
            
            CreateTable(
                "dbo._AuthorizationTokenStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.DocuSignEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalEventType = c.Int(nullable: false),
                        RecipientId = c.String(),
                        EnvelopeId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Fields",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        FieldColumnOffset = c.Int(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        MT_FieldType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_FieldType", t => t.MT_FieldType_Id)
                .ForeignKey("dbo.MT_Objects", t => t.MT_ObjectId, cascadeDelete: true)
                .Index(t => new { t.MT_ObjectId, t.Name, t.FieldColumnOffset }, name: "FieldColumnOffsetIndex")
                .Index(t => t.MT_FieldType_Id);
            
            CreateTable(
                "dbo.MT_FieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false),
                        AssemblyName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Objects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        MT_OrganizationId = c.Int(nullable: false),
                        MT_FieldType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_FieldType", t => t.MT_FieldType_Id)
                .ForeignKey("dbo.MT_Organizations", t => t.MT_OrganizationId, cascadeDelete: true)
                .Index(t => t.MT_OrganizationId)
                .Index(t => t.MT_FieldType_Id);
            
            CreateTable(
                "dbo.MT_Organizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Data",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GUID = c.Guid(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        fr8AccountId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Value1 = c.String(),
                        Value2 = c.String(),
                        Value3 = c.String(),
                        Value4 = c.String(),
                        Value5 = c.String(),
                        Value6 = c.String(),
                        Value7 = c.String(),
                        Value8 = c.String(),
                        Value9 = c.String(),
                        Value10 = c.String(),
                        Value11 = c.String(),
                        Value12 = c.String(),
                        Value13 = c.String(),
                        Value14 = c.String(),
                        Value15 = c.String(),
                        Value16 = c.String(),
                        Value17 = c.String(),
                        Value18 = c.String(),
                        Value19 = c.String(),
                        Value20 = c.String(),
                        Value21 = c.String(),
                        Value22 = c.String(),
                        Value23 = c.String(),
                        Value24 = c.String(),
                        Value25 = c.String(),
                        Value26 = c.String(),
                        Value27 = c.String(),
                        Value28 = c.String(),
                        Value29 = c.String(),
                        Value30 = c.String(),
                        Value31 = c.String(),
                        Value32 = c.String(),
                        Value33 = c.String(),
                        Value34 = c.String(),
                        Value35 = c.String(),
                        Value36 = c.String(),
                        Value37 = c.String(),
                        Value38 = c.String(),
                        Value39 = c.String(),
                        Value40 = c.String(),
                        Value41 = c.String(),
                        Value42 = c.String(),
                        Value43 = c.String(),
                        Value44 = c.String(),
                        Value45 = c.String(),
                        Value46 = c.String(),
                        Value47 = c.String(),
                        Value48 = c.String(),
                        Value49 = c.String(),
                        Value50 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Objects", t => t.MT_ObjectId, cascadeDelete: true)
                .Index(t => t.MT_ObjectId);
            
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocuSignTemplateID = c.Int(),
                        DocuSignEnvelopeID = c.Int(),
                        CloudStorageUrl = c.String(),
                        OriginalFileName = c.String(),
                        DockyardAccountID = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        TestAccount = c.Boolean(nullable: false),
                        Available = c.Boolean(),
                        EmailAddressID = c.Int(nullable: false),
                        DocusignAccountId = c.String(),
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
            
            CreateTable(
                "dbo.ProcessTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DockyardAccount_Id = c.String(maxLength: 128),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        ProcessTemplateState = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.Id)
                .ForeignKey("dbo.Users", t => t.DockyardAccount_Id)
                .ForeignKey("dbo._ProcessTemplateStateTemplate", t => t.ProcessTemplateState, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.DockyardAccount_Id)
                .Index(t => t.ProcessTemplateState);
            
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        CrateStorage = c.String(),
                        ActionState = c.Int(),
                        ActivityTemplateId = c.Int(),
                        currentView = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.Id)
                .ForeignKey("dbo._ActionStateTemplate", t => t.ActionState)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId)
                .Index(t => t.Id)
                .Index(t => t.ActionState)
                .Index(t => t.ActivityTemplateId);
            
            CreateTable(
                "dbo.ProcessNodeTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        StartingProcessNodeTemplate = c.Boolean(nullable: false),
                        NodeTransitions = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropForeignKey("dbo.Actions", "ActionState", "dbo._ActionStateTemplate");
            DropForeignKey("dbo.Actions", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ProcessTemplates", "ProcessTemplateState", "dbo._ProcessTemplateStateTemplate");
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Users", "State", "dbo._UserStateTemplate");
            DropForeignKey("dbo.Users", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Users", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Invitations", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            DropForeignKey("dbo.Invitations", "InvitationType", "dbo._InvitationTypeTemplate");
            DropForeignKey("dbo.Invitations", "Id", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "Id", "dbo.StoredFiles");
            DropForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Objects", "MT_OrganizationId", "dbo.MT_Organizations");
            DropForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.ExpectedResponses", "UserID", "dbo.Users");
            DropForeignKey("dbo.ExpectedResponses", "Status", "dbo._ExpectedResponseStatusTemplate");
            DropForeignKey("dbo.AuthorizationTokens", "UserID", "dbo.Users");
            DropForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins");
            DropForeignKey("dbo.AuthorizationTokens", "AuthorizationTokenState", "dbo._AuthorizationTokenStateTemplate");
            DropForeignKey("dbo.RemoteCalendarAuthData", "UserID", "dbo.Users");
            DropForeignKey("dbo.RemoteCalendarAuthData", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthType", "dbo._ServiceAuthorizationTypeTemplate");
            DropForeignKey("dbo.Concepts", "RequestId", "dbo.Emails");
            DropForeignKey("dbo.History", "CreatedByID", "dbo.Users");
            DropForeignKey("dbo.UserAgentInfos", "UserID", "dbo.Users");
            DropForeignKey("dbo.TrackingStatuses", "TrackingType", "dbo._TrackingTypeTemplate");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatus", "dbo._TrackingStatusTemplate");
            DropForeignKey("dbo.CommunicationConfigurations", "CommunicationType", "dbo._CommunicationTypeTemplate");
            DropForeignKey("dbo.Processes", "ProcessState", "dbo._ProcessStateTemplate");
            DropForeignKey("dbo.Processes", "NextActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Processes", "CurrentActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeState", "dbo._ProcessNodeStatusTemplate");
            DropForeignKey("dbo.ProcessNodes", "ParentProcessId", "dbo.Processes");
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Criteria", "CriteriaExecutionType", "dbo._CriteriaExecutionTypeTemplate");
            DropForeignKey("dbo.ActivityTemplate", "PluginID", "dbo.Plugins");
            DropForeignKey("dbo.ActivityDOes", "ParentActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Subscriptions", "DockyardAccountId", "dbo.Users");
            DropForeignKey("dbo.Subscriptions", "PluginId", "dbo.Plugins");
            DropForeignKey("dbo.Plugins", "PluginStatus", "dbo._PluginStatusTemplate");
            DropForeignKey("dbo.Subscriptions", "AccessLevel", "dbo._AccessLevelTemplate");
            DropForeignKey("dbo.Profiles", "DockyardAccountID", "dbo.Users");
            DropForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles");
            DropForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.Emails", "FromID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailParticipantType", "dbo._EmailParticipantTypeTemplate");
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Envelopes", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Emails", "EmailStatus", "dbo._EmailStatusTemplate");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" }, "dbo.AspNetUserRoles");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Id" });
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });
            DropIndex("dbo.Actions", new[] { "ActionState" });
            DropIndex("dbo.Actions", new[] { "Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "ProcessTemplateState" });
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "Id" });
            DropIndex("dbo.Users", new[] { "State" });
            DropIndex("dbo.Users", "IX_User_EmailAddress");
            DropIndex("dbo.Users", new[] { "Id" });
            DropIndex("dbo.Invitations", new[] { "ConfirmationStatus" });
            DropIndex("dbo.Invitations", new[] { "InvitationType" });
            DropIndex("dbo.Invitations", new[] { "Id" });
            DropIndex("dbo.Attachments", new[] { "EmailID" });
            DropIndex("dbo.Attachments", new[] { "Id" });
            DropIndex("dbo.MT_Data", new[] { "MT_ObjectId" });
            DropIndex("dbo.MT_Objects", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Objects", new[] { "MT_OrganizationId" });
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Fields", "FieldColumnOffsetIndex");
            DropIndex("dbo.ExpectedResponses", new[] { "Status" });
            DropIndex("dbo.ExpectedResponses", new[] { "UserID" });
            DropIndex("dbo.AuthorizationTokens", new[] { "AuthorizationTokenState" });
            DropIndex("dbo.AuthorizationTokens", new[] { "PluginID" });
            DropIndex("dbo.AuthorizationTokens", new[] { "UserID" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "UserID" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "ProviderID" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "AuthType" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "Name" });
            DropIndex("dbo.Concepts", new[] { "RequestId" });
            DropIndex("dbo.History", new[] { "CreatedByID" });
            DropIndex("dbo.UserAgentInfos", new[] { "UserID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingStatus" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingType" });
            DropIndex("dbo.CommunicationConfigurations", new[] { "CommunicationType" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeState" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcessId" });
            DropIndex("dbo.Criteria", new[] { "CriteriaExecutionType" });
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.ActivityTemplate", new[] { "PluginID" });
            DropIndex("dbo.ActivityDOes", new[] { "ParentActivityId" });
            DropIndex("dbo.Processes", new[] { "NextActivityId" });
            DropIndex("dbo.Processes", new[] { "CurrentActivityId" });
            DropIndex("dbo.Processes", new[] { "ProcessState" });
            DropIndex("dbo.Processes", new[] { "ProcessTemplateId" });
            DropIndex("dbo.Plugins", new[] { "PluginStatus" });
            DropIndex("dbo.Subscriptions", new[] { "AccessLevel" });
            DropIndex("dbo.Subscriptions", new[] { "PluginId" });
            DropIndex("dbo.Subscriptions", new[] { "DockyardAccountId" });
            DropIndex("dbo.ProfileItems", new[] { "ProfileNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ParentNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ProfileID" });
            DropIndex("dbo.Profiles", new[] { "DockyardAccountID" });
            DropIndex("dbo.Envelopes", new[] { "EmailID" });
            DropIndex("dbo.Emails", new[] { "FromID" });
            DropIndex("dbo.Emails", new[] { "EmailStatus" });
            DropIndex("dbo.Recipients", new[] { "EmailParticipantType" });
            DropIndex("dbo.Recipients", new[] { "EmailAddressID" });
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            DropIndex("dbo.EmailAddresses", "IX_EmailAddress_Address");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.ProcessNodeTemplates");
            DropTable("dbo.Actions");
            DropTable("dbo.ProcessTemplates");
            DropTable("dbo.Users");
            DropTable("dbo.Invitations");
            DropTable("dbo.Attachments");
            DropTable("dbo.Files");
            DropTable("dbo.MT_Data");
            DropTable("dbo.MT_Organizations");
            DropTable("dbo.MT_Objects");
            DropTable("dbo.MT_FieldType");
            DropTable("dbo.MT_Fields");
            DropTable("dbo.DocuSignEvents");
            DropTable("dbo._ExpectedResponseStatusTemplate");
            DropTable("dbo.ExpectedResponses");
            DropTable("dbo.ProfileNodeDescendantsCTEView");
            DropTable("dbo.ProfileNodeAncestorsCTEView");
            DropTable("dbo.Logs");
            DropTable("dbo._AuthorizationTokenStateTemplate");
            DropTable("dbo.AuthorizationTokens");
            DropTable("dbo.RemoteCalendarAuthData");
            DropTable("dbo._ServiceAuthorizationTypeTemplate");
            DropTable("dbo.RemoteCalendarProviders");
            DropTable("dbo.Concepts");
            DropTable("dbo.History");
            DropTable("dbo.UserAgentInfos");
            DropTable("dbo._TrackingTypeTemplate");
            DropTable("dbo._TrackingStatusTemplate");
            DropTable("dbo.TrackingStatuses");
            DropTable("dbo.Instructions");
            DropTable("dbo._CommunicationTypeTemplate");
            DropTable("dbo.CommunicationConfigurations");
            DropTable("dbo._ProcessStateTemplate");
            DropTable("dbo._ProcessTemplateStateTemplate");
            DropTable("dbo._ProcessNodeStatusTemplate");
            DropTable("dbo.ProcessNodes");
            DropTable("dbo._CriteriaExecutionTypeTemplate");
            DropTable("dbo.Criteria");
            DropTable("dbo.ActivityTemplate");
            DropTable("dbo._ActionStateTemplate");
            DropTable("dbo.ActivityDOes");
            DropTable("dbo.Processes");
            DropTable("dbo._UserStateTemplate");
            DropTable("dbo._PluginStatusTemplate");
            DropTable("dbo.Plugins");
            DropTable("dbo._AccessLevelTemplate");
            DropTable("dbo.Subscriptions");
            DropTable("dbo.ProfileItems");
            DropTable("dbo.ProfileNodes");
            DropTable("dbo.Profiles");
            DropTable("dbo._EmailParticipantTypeTemplate");
            DropTable("dbo._InvitationTypeTemplate");
            DropTable("dbo._ConfirmationStatusTemplate");
            DropTable("dbo.Envelopes");
            DropTable("dbo._EmailStatusTemplate");
            DropTable("dbo.StoredFiles");
            DropTable("dbo.Emails");
            DropTable("dbo.Recipients");
            DropTable("dbo.EmailAddresses");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}
