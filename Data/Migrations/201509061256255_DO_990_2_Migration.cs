namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO_990_2_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists");
            DropForeignKey("dbo.ActionLists", "ParentActionListId", "dbo.ActionLists");
            DropIndex("dbo.Actions", new[] { "ParentActionListId" });
            DropIndex("dbo.ActionLists", new[] { "ParentActionListId" });
            AddColumn("dbo.ActivityDOes", "ParentActivityId", c => c.Int());
            CreateIndex("dbo.ActivityDOes", "ParentActivityId");
            AddForeignKey("dbo.ActivityDOes", "ParentActivityId", "dbo.ActivityDOes", "Id");
            DropColumn("dbo.ActionLists", "ParentActionListId");
            DropColumn("dbo.Actions", "ParentActionListId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ParentActionListId", c => c.Int());
            AddColumn("dbo.ActionLists", "ParentActionListId", c => c.Int());
            DropForeignKey("dbo.ActivityDOes", "ParentActivityId", "dbo.ActivityDOes");
            DropIndex("dbo.ActivityDOes", new[] { "ParentActivityId" });
            DropColumn("dbo.ActivityDOes", "ParentActivityId");
            CreateIndex("dbo.ActionLists", "ParentActionListId");
            CreateIndex("dbo.Actions", "ParentActionListId");
            AddForeignKey("dbo.ActionLists", "ParentActionListId", "dbo.ActionLists", "Id");
            AddForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists", "Id");
        }
    }
}
