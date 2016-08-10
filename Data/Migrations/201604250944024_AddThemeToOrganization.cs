namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThemeToOrganization : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "LogoUrl", c => c.String());
            AddColumn("dbo.Organizations", "BackgroundColor", c => c.String());
            AddColumn("dbo.Organizations", "ThemeName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "ThemeName");
            DropColumn("dbo.Organizations", "BackgroundColor");
            DropColumn("dbo.Organizations", "LogoUrl");
        }
    }
}
