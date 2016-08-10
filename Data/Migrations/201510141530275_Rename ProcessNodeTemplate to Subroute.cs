namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameProcessNodeTemplatetoSubroute : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes");
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Id" });
            CreateTable(
                "dbo.Subroutes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        StartingSubroute = c.Boolean(nullable: false),
                        NodeTransitions = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityDOes", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.Criteria", "SubrouteId", c => c.Int());
            AddColumn("dbo.ProcessNodes", "SubrouteId", c => c.Int(nullable: false));
            CreateIndex("dbo.Criteria", "SubrouteId");
            CreateIndex("dbo.ProcessNodes", "SubrouteId");
            AddForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes", "Id");
            AddForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes", "Id");
            DropColumn("dbo.Criteria", "ProcessNodeTemplateId");
            DropColumn("dbo.ProcessNodes", "ProcessNodeTemplateId");
            DropTable("dbo.ProcessNodeTemplates");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProcessNodeTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        StartingProcessNodeTemplate = c.Boolean(nullable: false),
                        NodeTransitions = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ProcessNodes", "ProcessNodeTemplateId", c => c.Int(nullable: false));
            AddColumn("dbo.Criteria", "ProcessNodeTemplateId", c => c.Int());
            DropForeignKey("dbo.Subroutes", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes");
            DropForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes");
            DropIndex("dbo.Subroutes", new[] { "Id" });
            DropIndex("dbo.ProcessNodes", new[] { "SubrouteId" });
            DropIndex("dbo.Criteria", new[] { "SubrouteId" });
            DropColumn("dbo.ProcessNodes", "SubrouteId");
            DropColumn("dbo.Criteria", "SubrouteId");
            DropTable("dbo.Subroutes");
            CreateIndex("dbo.ProcessNodeTemplates", "Id");
            CreateIndex("dbo.ProcessNodes", "ProcessNodeTemplateId");
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateId");
            AddForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
