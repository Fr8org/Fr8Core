namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._ProcessStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Processes", "Description", c => c.String());
            AddColumn("dbo.Processes", "UserId", c => c.String());
            AddColumn("dbo.Processes", "ProcessState", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "ProcessState");
            AddForeignKey("dbo.Processes", "ProcessState", "dbo._ProcessStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Processes", "ProcessState", "dbo._ProcessStateTemplate");
            DropIndex("dbo.Processes", new[] { "ProcessState" });
            DropColumn("dbo.Processes", "ProcessState");
            DropColumn("dbo.Processes", "UserId");
            DropColumn("dbo.Processes", "Description");
            DropTable("dbo._ProcessStateTemplate");
        }
    }
}
