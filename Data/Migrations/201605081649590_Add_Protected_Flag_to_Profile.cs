namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Protected_Flag_to_Profile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "Protected", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "Protected");
        }
    }
}
