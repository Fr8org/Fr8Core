namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateProcessNodeTemplateDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            AlterColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            AlterColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int());
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id");
        }
    }
}
