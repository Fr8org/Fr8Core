namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUp_Migration_2 : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            // DropForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices");
            // DropIndex("dbo.ActivityTemplate", new[] { "WebServiceId" });
            // DropColumn("dbo.ActivityTemplate", "Category");
            // DropColumn("dbo.ActivityTemplate", "WebServiceId");
            // DropTable("dbo.WebServices");
        }
        
        public override void Down()
        {
            // CreateTable(
            //     "dbo.WebServices",
            //     c => new
            //         {
            //             Id = c.Int(nullable: false, identity: true),
            //             Name = c.String(),
            //             IconPath = c.String(),
            //             LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
            //             CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
            //         })
            //     .PrimaryKey(t => t.Id);
            // 
            // AddColumn("dbo.ActivityTemplate", "WebServiceId", c => c.Int());
            // AddColumn("dbo.ActivityTemplate", "Category", c => c.Int(nullable: false));
            // CreateIndex("dbo.ActivityTemplate", "WebServiceId");
            // AddForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices", "Id");
        }
    }
}
