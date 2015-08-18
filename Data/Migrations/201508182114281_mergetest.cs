namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mergetest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProcessTemplates", "DockyardAccount_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProcessTemplates", "DockyardAccount_Id");
            AddForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users", "Id");
            DropColumn("dbo.ProcessTemplates", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String(nullable: false));
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropColumn("dbo.ProcessTemplates", "DockyardAccount_Id");
        }
    }
}
