namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeProcessTemplatetoRoute : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo._ProcessTemplateStateTemplate", newName: "_RouteStateTemplate");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropForeignKey("dbo.ProcessTemplates", "ProcessTemplateState", "dbo._ProcessTemplateStateTemplate");
            DropIndex("dbo.Processes", new[] { "ProcessTemplateId" });
            DropIndex("dbo.ProcessTemplates", new[] { "Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "ProcessTemplateState" });
            CreateTable(
                "dbo.Routes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DockyardAccount_Id = c.String(maxLength: 128),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        RouteState = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.Id)
                .ForeignKey("dbo.Users", t => t.DockyardAccount_Id)
                .ForeignKey("dbo._RouteStateTemplate", t => t.RouteState, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.DockyardAccount_Id)
                .Index(t => t.RouteState);
            
            AddColumn("dbo.Processes", "RouteId", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "RouteId");
            AddForeignKey("dbo.Processes", "RouteId", "dbo.Routes", "Id");
            DropColumn("dbo.Processes", "ProcessTemplateId");
            DropTable("dbo.ProcessTemplates");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProcessTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DockyardAccount_Id = c.String(maxLength: 128),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        ProcessTemplateState = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Processes", "ProcessTemplateId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Routes", "RouteState", "dbo._RouteStateTemplate");
            DropForeignKey("dbo.Routes", "DockyardAccount_Id", "dbo.Users");
            DropForeignKey("dbo.Routes", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Processes", "RouteId", "dbo.Routes");
            DropIndex("dbo.Routes", new[] { "RouteState" });
            DropIndex("dbo.Routes", new[] { "DockyardAccount_Id" });
            DropIndex("dbo.Routes", new[] { "Id" });
            DropIndex("dbo.Processes", new[] { "RouteId" });
            DropColumn("dbo.Processes", "RouteId");
            DropTable("dbo.Routes");
            CreateIndex("dbo.ProcessTemplates", "ProcessTemplateState");
            CreateIndex("dbo.ProcessTemplates", "DockyardAccount_Id");
            CreateIndex("dbo.ProcessTemplates", "Id");
            CreateIndex("dbo.Processes", "ProcessTemplateId");
            AddForeignKey("dbo.ProcessTemplates", "ProcessTemplateState", "dbo._ProcessTemplateStateTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id");
            RenameTable(name: "dbo._RouteStateTemplate", newName: "_ProcessTemplateStateTemplate");
        }
    }
}
