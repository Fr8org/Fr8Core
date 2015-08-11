namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Criteria_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Criteria",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExecutionType = c.Int(nullable: false),
                        ConditionsJSON = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._CriteriaExecutionTypeTemplate", t => t.ExecutionType, cascadeDelete: true)
                .Index(t => t.ExecutionType);
            
            CreateTable(
                "dbo._CriteriaExecutionTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
                        
            AddColumn("dbo.ProcessNodeTemplates", "Criteria_Id", c => c.Int());
            CreateIndex("dbo.ProcessNodeTemplates", "Criteria_Id");
            AddForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria");
            DropForeignKey("dbo.Criteria", "ExecutionType", "dbo._CriteriaExecutionTypeTemplate");
            DropIndex("dbo.Criteria", new[] { "ExecutionType" });
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Criteria_Id" });
            DropColumn("dbo.ProcessNodeTemplates", "Criteria_Id");
            DropTable("dbo._CriteriaExecutionTypeTemplate");
            DropTable("dbo.Criteria");
        }
    }
}
