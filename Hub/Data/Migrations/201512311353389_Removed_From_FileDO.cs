namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removed_From_FileDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Files", "DocuSignTemplateID");
            DropColumn("dbo.Files", "DocuSignEnvelopeID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Files", "DocuSignEnvelopeID", c => c.Int());
            AddColumn("dbo.Files", "DocuSignTemplateID", c => c.Int());
        }
    }
}
