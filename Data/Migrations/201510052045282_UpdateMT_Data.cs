namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMT_Data : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MT_Data", "fr8AccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MT_Data", "fr8AccountId");
        }
    }
}
