using System;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Name = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128),
                    RoleId = c.String(nullable: false, maxLength: 128),
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
                "dbo.People",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    FirstName = c.String(maxLength: 30),
                    LastName = c.String(maxLength: 30),
                    EmailAddress_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddress_Id, cascadeDelete: true)
                .Index(t => t.EmailAddress_Id);

            CreateTable(
                "dbo.EmailAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Address = c.String(nullable: false, maxLength: 30),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Address, unique: true, name: "IX_EmailAddress_Address");

            CreateTable(
                "dbo.EmailEmailAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    EmailID = c.Int(nullable: false),
                    EmailAddressID = c.Int(nullable: false),
                    Type = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID, cascadeDelete: true)
                .Index(t => t.EmailID)
                .Index(t => t.EmailAddressID);

            CreateTable(
                "dbo.Emails",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Subject = c.String(),
                    HTMLText = c.String(),
                    PlainText = c.String(),
                    Status = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.StoredFiles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OriginalName = c.String(),
                    StoredName = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Events",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StartDate = c.DateTime(nullable: false),
                    EndDate = c.DateTime(nullable: false),
                    Location = c.String(),
                    Status = c.String(),
                    Transparency = c.String(),
                    Class = c.String(),
                    Description = c.String(),
                    Priority = c.Int(nullable: false),
                    Sequence = c.Int(nullable: false),
                    Summary = c.String(),
                    Category = c.String(),
                    IsAllDay = c.Boolean(nullable: false),
                    BookingRequest_Id = c.Int(),
                    CreatedBy_Id = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequest_Id)
                .ForeignKey("dbo.Users", t => t.CreatedBy_Id)
                .Index(t => t.BookingRequest_Id)
                .Index(t => t.CreatedBy_Id);

            CreateTable(
                "dbo.Attendees",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    EmailAddress = c.String(),
                    EventID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .Index(t => t.EventID);

            CreateTable(
                "dbo.Instructions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Category = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Calendars",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    PersonId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.People", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);

            CreateTable(
                "dbo.CommunicationConfigurations",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Type = c.Int(nullable: false),
                    ToAddress = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TrackingStatuses",
                c => new
                {
                    Id = c.Int(nullable: false),
                    ForeignTableName = c.String(nullable: false, maxLength: 128),
                    Type = c.Int(nullable: false),
                    Status = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.ForeignTableName });

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
                "dbo.Attachments",
                c => new
                {
                    Id = c.Int(nullable: false),
                    EmailID = c.Int(nullable: false),
                    Type = c.String(),
                    ContentID = c.String(),
                    BoundaryEmbedded = c.Boolean(nullable: false),
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
                    User_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.Id)
                .Index(t => t.User_Id);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    PersonID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.People", t => t.PersonID, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.PersonID);

        }

        private void DropTable(String tableName)
        {
            Sql(String.Format(@"IF OBJECT_ID('dbo.{0}', 'U') IS NOT NULL
  DROP TABLE dbo.{0}", tableName));
        }

        public override void Down()
        {
            DropForeignKey("dbo.Users", "PersonID", "dbo.People");
            DropForeignKey("dbo.Users", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.BookingRequests", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "Id", "dbo.StoredFiles");
            DropForeignKey("dbo.Calendars", "PersonId", "dbo.People");
            DropForeignKey("dbo.People", "EmailAddress_Id", "dbo.EmailAddresses");
            DropForeignKey("dbo.EmailEmailAddresses", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.EventEmail", "EventID", "dbo.Events");
            DropForeignKey("dbo.EventEmail", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Events", "CreatedBy_Id", "dbo.Users");
            DropForeignKey("dbo.Events", "BookingRequest_Id", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestInstruction", "InstructionID", "dbo.Instructions");
            DropForeignKey("dbo.BookingRequestInstruction", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropForeignKey("dbo.EmailEmailAddresses", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.Users", new[] { "PersonID" });
            DropIndex("dbo.Users", new[] { "Id" });
            DropIndex("dbo.BookingRequests", new[] { "User_Id" });
            DropIndex("dbo.BookingRequests", new[] { "Id" });
            DropIndex("dbo.Attachments", new[] { "EmailID" });
            DropIndex("dbo.Attachments", new[] { "Id" });
            DropIndex("dbo.EventEmail", new[] { "EventID" });
            DropIndex("dbo.EventEmail", new[] { "EmailID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "InstructionID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "BookingRequestID" });
            DropIndex("dbo.Calendars", new[] { "PersonId" });
            DropIndex("dbo.Attendees", new[] { "EventID" });
            DropIndex("dbo.Events", new[] { "CreatedBy_Id" });
            DropIndex("dbo.Events", new[] { "BookingRequest_Id" });
            DropIndex("dbo.EmailEmailAddresses", new[] { "EmailAddressID" });
            DropIndex("dbo.EmailEmailAddresses", new[] { "EmailID" });
            DropIndex("dbo.EmailAddresses", "IX_EmailAddress_Address");
            DropIndex("dbo.People", new[] { "EmailAddress_Id" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Users");
            DropTable("dbo.BookingRequests");
            DropTable("dbo.Attachments");
            DropTable("dbo.EventEmail");
            DropTable("dbo.BookingRequestInstruction");
            DropTable("dbo.TrackingStatuses");
            DropTable("dbo.CommunicationConfigurations");
            DropTable("dbo.Calendars");
            DropTable("dbo.Instructions");
            DropTable("dbo.Attendees");
            DropTable("dbo.Events");
            DropTable("dbo.StoredFiles");
            DropTable("dbo.Emails");
            DropTable("dbo.EmailEmailAddresses");
            DropTable("dbo.EmailAddresses");
            DropTable("dbo.People");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}
