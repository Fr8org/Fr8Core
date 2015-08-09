namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToProfileDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "Name");
        }
    }
}
