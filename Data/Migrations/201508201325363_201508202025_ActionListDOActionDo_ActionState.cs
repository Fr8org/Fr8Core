namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201508202025_ActionListDOActionDo_ActionState : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._ActionListStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ActionStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActionLists", "ActionListState", c => c.Int(nullable: false));
            AddColumn("dbo.Actions", "ActionState", c => c.Int(nullable: false));
            CreateIndex("dbo.ActionLists", "ActionListState");
            CreateIndex("dbo.Actions", "ActionState");
            AddForeignKey("dbo.ActionLists", "ActionListState", "dbo._ActionListStateTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Actions", "ActionState", "dbo._ActionStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActionState", "dbo._ActionStateTemplate");
            DropForeignKey("dbo.ActionLists", "ActionListState", "dbo._ActionListStateTemplate");
            DropIndex("dbo.Actions", new[] { "ActionState" });
            DropIndex("dbo.ActionLists", new[] { "ActionListState" });
            DropColumn("dbo.Actions", "ActionState");
            DropColumn("dbo.ActionLists", "ActionListState");
            DropTable("dbo._ActionStateTemplate");
            DropTable("dbo._ActionListStateTemplate");
        }
    }
}
