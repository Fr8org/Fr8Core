namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocusignExternalEventGetsTemplateId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalEventRegistrations", "DocuSignTemplateId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalEventRegistrations", "DocuSignTemplateId");
        }
    }
}
