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
                        ExecutionMode = c.Int(nullable: false),
                        ConditionsJSON = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ProcessNodeTemplates", "Criteria_Id", c => c.Int());
            CreateIndex("dbo.ProcessNodeTemplates", "Criteria_Id");
            AddForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "Criteria_Id", "dbo.Criteria");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Criteria_Id" });
            DropColumn("dbo.ProcessNodeTemplates", "Criteria_Id");
            DropTable("dbo.Criteria");
        }
    }
}
