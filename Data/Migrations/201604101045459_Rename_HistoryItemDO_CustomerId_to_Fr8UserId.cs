namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Rename_HistoryItemDO_CustomerId_to_Fr8UserId : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.History", name: "CustomerId", newName: "Fr8UserId");

        }

        public override void Down()
        {
            RenameColumn(table: "dbo.History", name: "Fr8UserId", newName: "CustomerId");

        }
    }
}
