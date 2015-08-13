namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActionRegistration",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActionType = c.String(),
                        Version = c.String(),
                        ParentPluginRegistration = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ActionRegistration");
        }
    }
}
