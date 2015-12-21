namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedComponentToHistoryItemDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.History", "Component", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.History", "Component");
        }
    }
}
