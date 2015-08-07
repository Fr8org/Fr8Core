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
                        ProcessTemplateId = c.Int(nullable: false),
                        Name = c.String(),
                        ExecutionMode = c.Int(nullable: false),
                        ConditionsJSON = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessTemplates", t => t.ProcessTemplateId, cascadeDelete: true)
                .Index(t => t.ProcessTemplateId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Criteria", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.Criteria", new[] { "ProcessTemplateId" });
            DropTable("dbo.Criteria");
        }
    }
}
