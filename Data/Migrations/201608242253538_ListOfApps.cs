namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ListOfApps : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plans", "IsApp", c => c.Boolean(nullable: false));
            AddColumn("dbo.Plans", "AppLaunchURL", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans", "AppLaunchURL");
            DropColumn("dbo.Plans", "IsApp");
        }
    }
}
