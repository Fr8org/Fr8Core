namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessNodeTemplatePropsNamingChange : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.ActivityDOes", new[] { "ProcessNodeTemplateDO_Id" });
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            DropColumn("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", c => c.Int());
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            CreateIndex("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
