namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnifiedNestingMigration_CriteriaCascadedDelete : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateId" });
            AlterColumn("dbo.Criteria", "ProcessNodeTemplateId", c => c.Int());
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateId" });
            AlterColumn("dbo.Criteria", "ProcessNodeTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateId");
        }
    }
}
