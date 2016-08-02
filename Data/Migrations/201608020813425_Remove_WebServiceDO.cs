namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_WebServiceDO : DbMigration
    {
        public override void Up()
        {
            // TODO: FR-4943, remove this.
            // AddColumn("dbo.ActivityCategory", "WebServiceId", c => c.Int(true));
            // Sql("INSERT INTO [dbo].[ActivityCategory]")

            DropForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices");
            DropIndex("dbo.ActivityTemplate", new[] { "WebServiceId" });
            DropColumn("dbo.ActivityTemplate", "WebServiceId");
            DropTable("dbo.WebServices");

            // TODO: FR-4943, remove this.
            // DropColumn("dbo.ActivityCategory", "WebServiceId");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.WebServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IconPath = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActivityTemplate", "WebServiceId", c => c.Int());
            CreateIndex("dbo.ActivityTemplate", "WebServiceId");
            AddForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices", "Id");
        }
    }
}
