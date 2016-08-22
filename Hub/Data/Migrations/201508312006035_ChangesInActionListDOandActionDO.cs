namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangesInActionListDOandActionDO : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ActivityDOes", newName: "Activities");
            RenameColumn(table: "dbo.Actions", name: "ActionListId", newName: "ParentActionListID");
            RenameIndex(table: "dbo.Actions", name: "IX_ActionListId", newName: "IX_ParentActionListID");
            AddColumn("dbo.ActionLists", "ParentActionListID", c => c.Int());
            CreateIndex("dbo.ActionLists", "ParentActionListID");
            AddForeignKey("dbo.ActionLists", "ParentActionListID", "dbo.ActionLists", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActionLists", "ParentActionListID", "dbo.ActionLists");
            DropIndex("dbo.ActionLists", new[] { "ParentActionListID" });
            DropColumn("dbo.ActionLists", "ParentActionListID");
            RenameIndex(table: "dbo.Actions", name: "IX_ParentActionListID", newName: "IX_ActionListId");
            RenameColumn(table: "dbo.Actions", name: "ParentActionListID", newName: "ActionListId");
            RenameTable(name: "dbo.Activities", newName: "ActivityDOes");
        }
    }
}
