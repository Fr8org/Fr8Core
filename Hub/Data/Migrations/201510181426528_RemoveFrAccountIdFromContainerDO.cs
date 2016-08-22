namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFrAccountIdFromContainerDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Containers", "Fr8AccountId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Containers", "Fr8AccountId", c => c.String());
        }
    }
}
