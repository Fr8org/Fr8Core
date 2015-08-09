namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFromName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "FromName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "FromName");
        }
    }
}
