namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessDO_AddedCurrentActivityAndNextActivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Processes", "CurrentActivityId", c => c.Int());
            AddColumn("dbo.Processes", "NextActivityId", c => c.Int());
            CreateIndex("dbo.Processes", "CurrentActivityId");
            CreateIndex("dbo.Processes", "NextActivityId");
            AddForeignKey("dbo.Processes", "CurrentActivityId", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Processes", "NextActivityId", "dbo.ActivityDOes", "Id");
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
