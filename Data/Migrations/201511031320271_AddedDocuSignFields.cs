namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDocuSignFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocuSignEvents", "RecipientEmail", c => c.String());
            AddColumn("dbo.DocuSignEvents", "DocumentName", c => c.String());
            AddColumn("dbo.DocuSignEvents", "TemplateName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocuSignEvents", "TemplateName");
            DropColumn("dbo.DocuSignEvents", "DocumentName");
            DropColumn("dbo.DocuSignEvents", "RecipientEmail");
        }
    }
}
