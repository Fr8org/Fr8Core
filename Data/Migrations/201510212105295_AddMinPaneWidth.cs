namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMinPaneWidth : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "MinPaneWidth", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "MinPaneWidth");
        }
    }
}
