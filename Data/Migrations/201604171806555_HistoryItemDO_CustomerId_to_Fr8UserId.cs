namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HistoryItemDO_CustomerId_to_Fr8UserId : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.History", "CustomerId", "Fr8UserId");
        }

        public override void Down()
        {
            RenameColumn("dbo.History", "Fr8UserId", "CustomerId");
        }
    }
}
