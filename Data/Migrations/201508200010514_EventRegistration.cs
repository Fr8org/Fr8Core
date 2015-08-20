namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventRegistration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProcessTemplates", "DockyardAccount_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.ExternalEventRegistrations", "ExternalEvent", c => c.Int(nullable: false));
            AddColumn("dbo.Envelopes", "EnvelopeStatus", c => c.Int(nullable: false));
            CreateIndex("dbo.ProcessTemplates", "DockyardAccount_Id");
            CreateIndex("dbo.ExternalEventRegistrations", "ExternalEvent");
            AddForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.ExternalEventRegistrations", "ExternalEvent", "dbo._EventStatusTemplate", "Id", cascadeDelete: true);
            DropColumn("dbo.ProcessTemplates", "UserId");
            DropColumn("dbo.ExternalEventRegistrations", "EventType");
            DropColumn("dbo.Envelopes", "Status");
            DropColumn("dbo.Envelopes", "LastUpdated");
            DropColumn("dbo.Envelopes", "CreateDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Envelopes", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Envelopes", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Envelopes", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.ExternalEventRegistrations", "EventType", c => c.Int(nullable: false));
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String(nullable: false));
            DropForeignKey("dbo.ExternalEventRegistrations", "ExternalEvent", "dbo._EventStatusTemplate");
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropIndex("dbo.ExternalEventRegistrations", new[] { "ExternalEvent" });
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropColumn("dbo.Envelopes", "EnvelopeStatus");
            DropColumn("dbo.ExternalEventRegistrations", "ExternalEvent");
            DropColumn("dbo.ProcessTemplates", "DockyardAccount_Id");
        }
    }
}
