namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnifiedNestingMigration_removePNTfromActionDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.Actions", new[] { "ProcessNodeTemplateDO_Id" });
            DropColumn("dbo.Actions", "ProcessNodeTemplateDO_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ProcessNodeTemplateDO_Id", c => c.Int());
            CreateIndex("dbo.Actions", "ProcessNodeTemplateDO_Id");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
