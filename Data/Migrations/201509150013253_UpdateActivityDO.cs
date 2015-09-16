namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActivityDO : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.Plugins", "Endpoint", c => c.String());
           // DropColumn("dbo.ActivityTemplate", "DefaultEndPoint");
           //DropColumn("dbo.ActivityTemplate", "ActionProcessor");
        }

        public override void Down()
        {
            //AddColumn("dbo.ActivityTemplate", "DefaultEndPoint", c => c.String());
            //AddColumn("dbo.ActivityTemplate", "ActionProcessor", c => c.String());
        }
    }
}
