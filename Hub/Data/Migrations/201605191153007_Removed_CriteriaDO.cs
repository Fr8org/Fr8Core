namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removed_CriteriaDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Criteria", "CriteriaExecutionType", "dbo._CriteriaExecutionTypeTemplate");
            DropForeignKey("dbo.Criteria", "SubPlanId", "dbo.SubPlans");
            DropIndex("dbo.Criteria", new[] { "SubPlanId" });
            DropIndex("dbo.Criteria", new[] { "CriteriaExecutionType" });
            DropTable("dbo.Criteria");
            DropTable("dbo._CriteriaExecutionTypeTemplate");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo._CriteriaExecutionTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Criteria",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubPlanId = c.Guid(),
                        CriteriaExecutionType = c.Int(nullable: false),
                        ConditionsJSON = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Criteria", "CriteriaExecutionType");
            CreateIndex("dbo.Criteria", "SubPlanId");
            AddForeignKey("dbo.Criteria", "SubPlanId", "dbo.SubPlans", "Id");
            AddForeignKey("dbo.Criteria", "CriteriaExecutionType", "dbo._CriteriaExecutionTypeTemplate", "Id", cascadeDelete: true);
        }
    }
}
