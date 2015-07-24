namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Actions_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ActionListID = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActionLists", t => t.ActionListID)
                .Index(t => t.ActionListID);
            
            CreateTable(
                "dbo.ActionLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        TemplateId = c.Int(),
                        ProcessID = c.Int(),
                        TriggerEventID = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Processes", t => t.ProcessID)
                .ForeignKey("dbo.Templates", t => t.TemplateId)
                .ForeignKey("dbo._EventStatusTemplate", t => t.TriggerEventID)
                .Index(t => t.TemplateId)
                .Index(t => t.ProcessID)
                .Index(t => t.TriggerEventID);
            
            CreateTable(
                "dbo.Templates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.Envelopes", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActionLists", "TriggerEventID", "dbo._EventStatusTemplate");
            DropForeignKey("dbo.ActionLists", "TemplateId", "dbo.Templates");
            DropForeignKey("dbo.ActionLists", "ProcessID", "dbo.Processes");
            DropForeignKey("dbo.Actions", "ActionListID", "dbo.ActionLists");
            DropIndex("dbo.ActionLists", new[] { "TriggerEventID" });
            DropIndex("dbo.ActionLists", new[] { "ProcessID" });
            DropIndex("dbo.ActionLists", new[] { "TemplateId" });
            DropIndex("dbo.Actions", new[] { "ActionListID" });
            AlterColumn("dbo.Envelopes", "Status", c => c.String());
            DropTable("dbo.Templates");
            DropTable("dbo.ActionLists");
            DropTable("dbo.Actions");
        }
    }
}
