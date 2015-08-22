namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class foo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Processes", "ParentProcessTemplateId", c => c.Int(nullable: false));
            AddColumn("dbo.ProcessNodes", "ProcessTemplateDO_Id", c => c.Int());
            CreateIndex("dbo.Processes", "ParentProcessTemplateId");
            CreateIndex("dbo.ProcessNodes", "ProcessTemplateDO_Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessTemplateDO_Id", "dbo.ProcessTemplates", "Id");
            AddForeignKey("dbo.Processes", "ParentProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Processes", "ParentProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessTemplateDO_Id", "dbo.ProcessTemplates");
            DropIndex("dbo.ProcessNodes", new[] { "ProcessTemplateDO_Id" });
            DropIndex("dbo.Processes", new[] { "ParentProcessTemplateId" });
            DropColumn("dbo.ProcessNodes", "ProcessTemplateDO_Id");
            DropColumn("dbo.Processes", "ParentProcessTemplateId");
        }
    }
}
