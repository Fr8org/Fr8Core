namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveActionList : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActionLists", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ActionLists", "ProcessID", "dbo.Processes");
            DropForeignKey("dbo.ActionLists", "ActionListType", "dbo._ActionListTypeTemplate");
            DropForeignKey("dbo.ActionLists", "CurrentActivityID", "dbo.ActivityDOes");
            DropForeignKey("dbo.ActionLists", "ActionListState", "dbo._ActionListStateTemplate");
            DropIndex("dbo.ActionLists", new[] { "Id" });
            DropIndex("dbo.ActionLists", new[] { "ProcessNodeTemplateDO_Id" });
            DropIndex("dbo.ActionLists", new[] { "ProcessID" });
            DropIndex("dbo.ActionLists", new[] { "ActionListType" });
            DropIndex("dbo.ActionLists", new[] { "CurrentActivityID" });
            DropIndex("dbo.ActionLists", new[] { "ActionListState" });
            AddColumn("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", c => c.Int());
            AddColumn("dbo.Actions", "ProcessNodeTemplateDO_Id", c => c.Int());
            CreateIndex("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id");
            CreateIndex("dbo.Actions", "ProcessNodeTemplateDO_Id");
            AddForeignKey("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            DropTable("dbo._ActionListStateTemplate");
            DropTable("dbo._ActionListTypeTemplate");
            DropTable("dbo.ActionLists");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ActionLists",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        ProcessNodeTemplateDO_Id = c.Int(),
                        ProcessID = c.Int(),
                        ActionListType = c.Int(nullable: false),
                        CurrentActivityID = c.Int(),
                        ActionListState = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ActionListTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ActionListStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.Actions", new[] { "ProcessNodeTemplateDO_Id" });
            DropIndex("dbo.ActivityDOes", new[] { "ProcessNodeTemplateDO_Id" });
            DropColumn("dbo.Actions", "ProcessNodeTemplateDO_Id");
            DropColumn("dbo.ActivityDOes", "ProcessNodeTemplateDO_Id");
            CreateIndex("dbo.ActionLists", "ActionListState");
            CreateIndex("dbo.ActionLists", "CurrentActivityID");
            CreateIndex("dbo.ActionLists", "ActionListType");
            CreateIndex("dbo.ActionLists", "ProcessID");
            CreateIndex("dbo.ActionLists", "ProcessNodeTemplateDO_Id");
            CreateIndex("dbo.ActionLists", "Id");
            AddForeignKey("dbo.ActionLists", "ActionListState", "dbo._ActionListStateTemplate", "Id");
            AddForeignKey("dbo.ActionLists", "CurrentActivityID", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ActionLists", "ActionListType", "dbo._ActionListTypeTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ActionLists", "ProcessID", "dbo.Processes", "Id");
            AddForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ActionLists", "Id", "dbo.ActivityDOes", "Id");
        }
    }
}
