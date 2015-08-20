namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocuSignTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProcessNodeTemplates", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessNodeTemplates", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProcessNodeTemplates", "CreateDate");
            DropColumn("dbo.ProcessNodeTemplates", "LastUpdated");
        }
    }
}
