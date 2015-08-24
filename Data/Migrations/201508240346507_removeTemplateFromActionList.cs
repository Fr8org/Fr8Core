namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeTemplateFromActionList : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActionLists", "TemplateId", "dbo.Templates");
            DropIndex("dbo.ActionLists", new[] { "TemplateId" });
            DropColumn("dbo.ActionLists", "TemplateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActionLists", "TemplateId", c => c.Int());
            CreateIndex("dbo.ActionLists", "TemplateId");
            AddForeignKey("dbo.ActionLists", "TemplateId", "dbo.Templates", "Id");
        }
    }
}
