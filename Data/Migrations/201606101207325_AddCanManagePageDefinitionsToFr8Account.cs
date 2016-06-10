namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCanManagePageDefinitionsToFr8Account : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CanManagePageDefinitions", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "CanManagePageDefinitions");
        }
    }
}
