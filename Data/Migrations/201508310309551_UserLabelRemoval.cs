namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UserLabelRemoval : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Actions", "UserLabel");
        }

        public override void Down()
        {
            AddColumn("dbo.Actions", "UserLabel", c => c.String());
        }
    }
}
