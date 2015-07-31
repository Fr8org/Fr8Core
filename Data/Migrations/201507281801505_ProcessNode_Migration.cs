namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessNode_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProcessNodeTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        ProcessTemplateID = c.Int(),
                        TransitionKey = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessTemplates", t => t.ProcessTemplateID)
                .Index(t => t.ProcessTemplateID);
            
            CreateTable(
                "dbo.ProcessNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        State = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        ParentProcess_Id = c.Int(),
                        ProcessNodeTemplate_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Processes", t => t.ParentProcess_Id)
                .ForeignKey("dbo.ProcessNodeTemplates", t => t.ProcessNodeTemplate_Id)
                .Index(t => t.ParentProcess_Id)
                .Index(t => t.ProcessNodeTemplate_Id);
            
            AddColumn("dbo.Processes", "StartingProcessNodeTemplate_Id", c => c.Int());
            AddColumn("dbo.ActionLists", "ProcessNodeTemplateDO_Id", c => c.Int());
            CreateIndex("dbo.Processes", "StartingProcessNodeTemplate_Id");
            CreateIndex("dbo.ActionLists", "ProcessNodeTemplateDO_Id");
            AddForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.Processes", "StartingProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ParentProcess_Id", "dbo.Processes");
            DropForeignKey("dbo.Processes", "StartingProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodeTemplates", "ProcessTemplateID", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeTemplate_Id" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcess_Id" });
            DropIndex("dbo.ActionLists", new[] { "ProcessNodeTemplateDO_Id" });
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ProcessTemplateID" });
            DropIndex("dbo.Processes", new[] { "StartingProcessNodeTemplate_Id" });
            DropColumn("dbo.ActionLists", "ProcessNodeTemplateDO_Id");
            DropColumn("dbo.Processes", "StartingProcessNodeTemplate_Id");
            DropTable("dbo.ProcessNodes");
            DropTable("dbo.ProcessNodeTemplates");
        }
    }
}
