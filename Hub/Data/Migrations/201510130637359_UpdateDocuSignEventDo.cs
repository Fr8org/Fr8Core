namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDocuSignEventDo : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocuSignEvents", "DocuSignObject", c => c.String());
            AddColumn("dbo.DocuSignEvents", "Status", c => c.String());
            AddColumn("dbo.DocuSignEvents", "CreateDate", c => c.String());
            AddColumn("dbo.DocuSignEvents", "SentDate", c => c.String());
            AddColumn("dbo.DocuSignEvents", "DeliveredDate", c => c.String());
            AddColumn("dbo.DocuSignEvents", "CompletedDate", c => c.String());
            AddColumn("dbo.DocuSignEvents", "Email", c => c.String());
            AddColumn("dbo.DocuSignEvents", "EventId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocuSignEvents", "EventId");
            DropColumn("dbo.DocuSignEvents", "Email");
            DropColumn("dbo.DocuSignEvents", "CompletedDate");
            DropColumn("dbo.DocuSignEvents", "DeliveredDate");
            DropColumn("dbo.DocuSignEvents", "SentDate");
            DropColumn("dbo.DocuSignEvents", "CreateDate");
            DropColumn("dbo.DocuSignEvents", "Status");
            DropColumn("dbo.DocuSignEvents", "DocuSignObject");
        }
    }
}
