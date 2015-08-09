using Data.States;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemoteCalendar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventSyncStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            Sql(@"SET IDENTITY_INSERT dbo.EventSyncStatus ON;");
            Sql(string.Format("INSERT dbo.EventSyncStatus (Id, Name) VALUES ({0}, N'DoNotSync')", EventSyncState.DoNotSync));
            Sql(string.Format("INSERT dbo.EventSyncStatus (Id, Name) VALUES ({0}, N'SyncWithExternal')", EventSyncState.SyncWithExternal));
            Sql(@"SET IDENTITY_INSERT dbo.EventSyncStatus OFF;");

            CreateTable(
                "dbo.RemoteCalendarAuthData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProviderID = c.Int(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        AuthData = c.String(),
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
                        AuthTypeID = c.Int(nullable: false),
                        AppCreds = c.String(),
                        CalDAVEndPoint = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceAuthorizationTypes", t => t.AuthTypeID, cascadeDelete: true)
                .Index(t => t.Name, unique: true)
                .Index(t => t.AuthTypeID);
            
            CreateTable(
                "dbo.ServiceAuthorizationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RemoteCalendarLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RemoteCalendarName = c.String(),
                        LocalCalendarID = c.Int(nullable: false),
                        ProviderID = c.Int(nullable: false),
                        DateSynchronizationAttempted = c.DateTimeOffset(nullable: false, precision: 7),
                        DateSynchronized = c.DateTimeOffset(nullable: false, precision: 7),
                        LastSynchronizationResult = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Calendars", t => t.LocalCalendarID, cascadeDelete: true)
                .ForeignKey("dbo.RemoteCalendarProviders", t => t.ProviderID, cascadeDelete: true)
                .Index(t => t.LocalCalendarID)
                .Index(t => t.ProviderID);

            AddColumn("dbo.Events", "SyncStatusID", c => c.Int(nullable: false, defaultValue: EventSyncState.DoNotSync));
            CreateIndex("dbo.Events", "SyncStatusID");
            AddForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus", "Id", cascadeDelete: true);
            DropColumn("dbo.Users", "GoogleAuthData");
            DropColumn("dbo.Calendars", "LastSynchronized");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Calendars", "LastSynchronized", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Users", "GoogleAuthData", c => c.String());
            DropForeignKey("dbo.RemoteCalendarLinks", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarLinks", "LocalCalendarID", "dbo.Calendars");
            DropForeignKey("dbo.RemoteCalendarAuthData", "UserID", "dbo.Users");
            DropForeignKey("dbo.RemoteCalendarAuthData", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes");
            DropForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus");
            DropIndex("dbo.RemoteCalendarLinks", new[] { "ProviderID" });
            DropIndex("dbo.RemoteCalendarLinks", new[] { "LocalCalendarID" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "AuthTypeID" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "Name" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "UserID" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "ProviderID" });
            DropIndex("dbo.Events", new[] { "SyncStatusID" });
            DropColumn("dbo.Events", "SyncStatusID");
            DropTable("dbo.RemoteCalendarLinks");
            DropTable("dbo.ServiceAuthorizationTypes");
            DropTable("dbo.RemoteCalendarProviders");
            DropTable("dbo.RemoteCalendarAuthData");
            DropTable("dbo.EventSyncStatus");
        }
    }
}
