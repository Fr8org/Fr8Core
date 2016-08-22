namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTypeToActivityCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityCategory", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityCategory", "Type");
        }
    }
}
