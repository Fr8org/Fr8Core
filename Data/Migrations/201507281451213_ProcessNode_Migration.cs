namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessNode_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProcessNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProcessID = c.Int(),
                        State = c.Int(nullable: false),
                        ProcessStateId = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Processes", t => t.ProcessID)
                .ForeignKey("dbo._ProcessStateTemplate", t => t.ProcessStateId, cascadeDelete: true)
                .Index(t => t.ProcessID)
                .Index(t => t.ProcessStateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodes", "ProcessStateId", "dbo._ProcessStateTemplate");
            DropForeignKey("dbo.ProcessNodes", "ProcessID", "dbo.Processes");
            DropIndex("dbo.ProcessNodes", new[] { "ProcessStateId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessID" });
            DropTable("dbo.ProcessNodes");
        }
    }
}
