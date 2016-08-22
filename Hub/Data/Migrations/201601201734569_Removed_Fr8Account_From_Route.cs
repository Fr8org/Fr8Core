namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Removed_Fr8Account_From_Route : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Routes", new[] { "Fr8Account_Id" });
            DropForeignKey("dbo.Routes", "Fr8Account_Id", "dbo.Users");
            DropColumn("dbo.Routes", "Fr8Account_Id");
            RenameColumn("dbo.RouteNodes", "Fr8Account_TEMP_Id", "Fr8AccountId");
            DropIndex(table: "dbo.RouteNodes", name: "Fr8Account_TEMP_Id");
            CreateIndex("dbo.RouteNodes", "Fr8AccountId");

        }

        public override void Down()
        {
            RenameColumn("dbo.RouteNodes", "Fr8AccountId", "Fr8Account_TEMP_Id");
            AddColumn("dbo.Routes", "Fr8Account_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Routes", "Fr8Account_Id");
            AddForeignKey("dbo.Route", "Fr8Account_Id", "dbo.Users");
        }
    }
}
