namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddNameToActivity : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "Name", c => c.String());

            Sql(@"
                  UPDATE dbo.Actions
                  SET Name = Label
                ");
            Sql(@"
                  UPDATE dbo.Actions
                  SET Label = NULL
                ");
        }

        public override void Down()
        {
            Sql(@"
                   UPDATE dbo.Actions
                   SET Label = Name
                 ");
            Sql(@"
                   UPDATE dbo.Actions
                   SET Name = NULL
                 ");

            DropColumn("dbo.Actions", "Name");
        }
    }
}
