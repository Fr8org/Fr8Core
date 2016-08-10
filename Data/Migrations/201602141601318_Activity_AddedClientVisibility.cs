namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Activity_AddedClientVisibility : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "ClientVisibility", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "ClientVisibility");
        }
    }
}
