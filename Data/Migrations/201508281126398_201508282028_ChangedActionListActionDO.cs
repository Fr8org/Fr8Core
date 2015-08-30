namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201508282028_ChangedActionListActionDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActionLists", "CurrentActionID", "dbo.Actions");
            DropForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists");
            DropIndex("dbo.ActionLists", new[] { "CurrentActionID" });
            DropPrimaryKey("dbo.ActionLists");
            DropPrimaryKey("dbo.Actions");
            CreateTable(
                "dbo.ActivityDOes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ordering = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActionLists", "CurrentActivityID", c => c.Int());
            AlterColumn("dbo.ActionLists", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Actions", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.ActionLists", "Id");
            AddPrimaryKey("dbo.Actions", "Id");
            CreateIndex("dbo.Actions", "Id");
            CreateIndex("dbo.ActionLists", "Id");
            CreateIndex("dbo.ActionLists", "CurrentActivityID");
            AddForeignKey("dbo.Actions", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ActionLists", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ActionLists", "CurrentActivityID", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists", "Id");
            DropColumn("dbo.ActionLists", "CurrentActionID");
            DropColumn("dbo.ActionLists", "LastUpdated");
            DropColumn("dbo.ActionLists", "CreateDate");
            DropColumn("dbo.Actions", "Ordering");
            DropColumn("dbo.Actions", "LastUpdated");
            DropColumn("dbo.Actions", "CreateDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Actions", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Actions", "Ordering", c => c.Int(nullable: false));
            AddColumn("dbo.ActionLists", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ActionLists", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ActionLists", "CurrentActionID", c => c.Int());
            DropForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists");
            DropForeignKey("dbo.ActionLists", "CurrentActivityID", "dbo.ActivityDOes");
            DropForeignKey("dbo.ActionLists", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Actions", "Id", "dbo.ActivityDOes");
            DropIndex("dbo.ActionLists", new[] { "CurrentActivityID" });
            DropIndex("dbo.ActionLists", new[] { "Id" });
            DropIndex("dbo.Actions", new[] { "Id" });
            DropPrimaryKey("dbo.Actions");
            DropPrimaryKey("dbo.ActionLists");
            AlterColumn("dbo.Actions", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ActionLists", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.ActionLists", "CurrentActivityID");
            DropTable("dbo.ActivityDOes");
            AddPrimaryKey("dbo.Actions", "Id");
            AddPrimaryKey("dbo.ActionLists", "Id");
            CreateIndex("dbo.ActionLists", "CurrentActionID");
            AddForeignKey("dbo.Actions", "ActionListId", "dbo.ActionLists", "Id");
            AddForeignKey("dbo.ActionLists", "CurrentActionID", "dbo.Actions", "Id");
        }
    }
}
