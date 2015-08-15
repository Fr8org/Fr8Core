namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CriteriaForeignKey_Move_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Criteria_Id" });
            AddColumn("dbo.Criteria", "ProcessNodeTemplateID", c => c.Int());
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateID");
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateID", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            DropColumn("dbo.ProcessNodeTemplates", "Criteria_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessNodeTemplates", "Criteria_Id", c => c.Int());
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateID", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateID" });
            DropColumn("dbo.Criteria", "ProcessNodeTemplateID");
            CreateIndex("dbo.ProcessNodeTemplates", "Criteria_Id");
            AddForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria", "Id");
        }
    }
}
