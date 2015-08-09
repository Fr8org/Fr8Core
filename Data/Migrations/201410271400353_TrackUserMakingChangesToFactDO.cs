namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackUserMakingChangesToFactDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Facts", "CreatedByID", c => c.String(maxLength: 128));
            CreateIndex("dbo.Facts", "CreatedByID");
            AddForeignKey("dbo.Facts", "CreatedByID", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Facts", "CreatedByID", "dbo.Users");
            DropIndex("dbo.Facts", new[] { "CreatedByID" });
            DropColumn("dbo.Facts", "CreatedByID");
        }
    }
}
