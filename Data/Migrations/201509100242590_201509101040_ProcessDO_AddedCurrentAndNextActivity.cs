namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201509101040_ProcessDO_AddedCurrentAndNextActivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Processes", "CurrentActivityId", c => c.Int(nullable: false));
            AddColumn("dbo.Processes", "NextActivityId", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "CurrentActivityId");
            CreateIndex("dbo.Processes", "NextActivityId");
            AddForeignKey("dbo.Processes", "CurrentActivityId", "dbo.ActivityDOes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Processes", "NextActivityId", "dbo.ActivityDOes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Processes", "NextActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Processes", "CurrentActivityId", "dbo.ActivityDOes");
            DropIndex("dbo.Processes", new[] { "NextActivityId" });
            DropIndex("dbo.Processes", new[] { "CurrentActivityId" });
            DropColumn("dbo.Processes", "NextActivityId");
            DropColumn("dbo.Processes", "CurrentActivityId");
        }
    }
}
