namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveActionState : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActionState", "dbo._ActionStateTemplate");
            DropIndex("dbo.Actions", new[] { "ActionState" });
            DropColumn("dbo.Actions", "ActionState");
            DropTable("dbo._ActionStateTemplate");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo._ActionStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Actions", "ActionState", c => c.Int());
            CreateIndex("dbo.Actions", "ActionState");
            AddForeignKey("dbo.Actions", "ActionState", "dbo._ActionStateTemplate", "Id");
        }
    }
}
