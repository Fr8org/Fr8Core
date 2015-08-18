namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merge1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalEventRegistrations", "ExternalEvent", c => c.Int(nullable: false));
            CreateIndex("dbo.ExternalEventRegistrations", "ExternalEvent");
            AddForeignKey("dbo.ExternalEventRegistrations", "ExternalEvent", "dbo._EventStatusTemplate", "Id", cascadeDelete: true);
            DropColumn("dbo.ExternalEventRegistrations", "EventType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalEventRegistrations", "EventType", c => c.Int(nullable: false));
            DropForeignKey("dbo.ExternalEventRegistrations", "ExternalEvent", "dbo._EventStatusTemplate");
            DropIndex("dbo.ExternalEventRegistrations", new[] { "ExternalEvent" });
            DropColumn("dbo.ExternalEventRegistrations", "ExternalEvent");
        }
    }
}
