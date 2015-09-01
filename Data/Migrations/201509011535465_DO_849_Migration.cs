namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO_849_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate");
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalEvent" });
            AlterColumn("dbo.ExternalEventSubscriptions", "ExternalEvent", c => c.Int());
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalEvent");
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate");
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalEvent" });
            AlterColumn("dbo.ExternalEventSubscriptions", "ExternalEvent", c => c.Int(nullable: false));
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalEvent");
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate", "Id", cascadeDelete: true);
        }
    }
}
