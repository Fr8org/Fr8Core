namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovingUnusedObjects : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthType", "dbo._ServiceAuthorizationTypeTemplate");
            DropForeignKey("dbo.RemoteCalendarAuthData", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.RemoteCalendarAuthData", "UserID", "dbo.Users");
            DropIndex("dbo.RemoteCalendarProviders", new[] { "Name" });
            DropIndex("dbo.RemoteCalendarProviders", new[] { "AuthType" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "ProviderID" });
            DropIndex("dbo.RemoteCalendarAuthData", new[] { "UserID" });
            DropTable("dbo.RemoteCalendarProviders");
            DropTable("dbo._ServiceAuthorizationTypeTemplate");
            DropTable("dbo.RemoteCalendarAuthData");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ServiceAuthorizationTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.RemoteCalendarAuthData", "UserID");
            CreateIndex("dbo.RemoteCalendarAuthData", "ProviderID");
            CreateIndex("dbo.RemoteCalendarProviders", "AuthType");
            CreateIndex("dbo.RemoteCalendarProviders", "Name", unique: true);
            AddForeignKey("dbo.RemoteCalendarAuthData", "UserID", "dbo.Users", "Id");
            AddForeignKey("dbo.RemoteCalendarAuthData", "ProviderID", "dbo.RemoteCalendarProviders", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarProviders", "AuthType", "dbo._ServiceAuthorizationTypeTemplate", "Id", cascadeDelete: true);
        }
    }
}
