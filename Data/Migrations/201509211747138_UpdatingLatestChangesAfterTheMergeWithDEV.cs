namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingLatestChangesAfterTheMergeWithDEV : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.History", "BookerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.History", "BookerId", c => c.String());
        }
    }
}
