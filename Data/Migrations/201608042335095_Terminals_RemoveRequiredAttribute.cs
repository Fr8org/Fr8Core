namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Terminals_RemoveRequiredAttribute : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Terminals", "Name", c => c.String());
            AlterColumn("dbo.Terminals", "Version", c => c.String());
            AlterColumn("dbo.Terminals", "Label", c => c.String());
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int());
        }

        public override void Down()
        {
            AlterColumn("dbo.Terminals", "Label", c => c.String(nullable: false));
            AlterColumn("dbo.Terminals", "Version", c => c.String(nullable: false));
            AlterColumn("dbo.Terminals", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int(nullable: false));
        }
    }
}
