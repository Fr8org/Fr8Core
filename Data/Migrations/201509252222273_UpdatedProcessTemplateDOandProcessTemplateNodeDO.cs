namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedProcessTemplateDOandProcessTemplateNodeDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            DropIndex("dbo.ProcessTemplates", new[] { "StartingProcessNodeTemplateId" });
            AddColumn("dbo.ProcessNodeTemplates", "StartingProcessNodeTemplate", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            DropColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            AlterColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int());
            DropColumn("dbo.ProcessNodeTemplates", "StartingProcessNodeTemplate");
            CreateIndex("dbo.ProcessTemplates", "StartingProcessNodeTemplateId");
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id");
            AddForeignKey("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
