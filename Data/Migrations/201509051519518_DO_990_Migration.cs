namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO_990_Migration : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Actions", name: "ActionListId", newName: "ParentActionListId");
            RenameIndex(table: "dbo.Actions", name: "IX_ActionListId", newName: "IX_ParentActionListId");
            AddColumn("dbo.ActionLists", "ParentActionListId", c => c.Int());
            CreateIndex("dbo.ActionLists", "ParentActionListId");
            AddForeignKey("dbo.ActionLists", "ParentActionListId", "dbo.ActionLists", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActionLists", "ParentActionListId", "dbo.ActionLists");
            DropIndex("dbo.ActionLists", new[] { "ParentActionListId" });
            DropColumn("dbo.ActionLists", "ParentActionListId");
            RenameIndex(table: "dbo.Actions", name: "IX_ParentActionListId", newName: "IX_ActionListId");
            RenameColumn(table: "dbo.Actions", name: "ParentActionListId", newName: "ActionListId");
        }
    }
}
