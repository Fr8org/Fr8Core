namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Description_ActivityTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Description");
        }
    }
}
