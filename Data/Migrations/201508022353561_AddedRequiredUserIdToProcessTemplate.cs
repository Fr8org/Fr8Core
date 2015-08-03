namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRequiredUserIdToProcessTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProcessTemplates", "UserId");
        }
    }
}
