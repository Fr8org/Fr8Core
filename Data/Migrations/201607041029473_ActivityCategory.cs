namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActivityCategory : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActivityCategory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(maxLength: 200),
                        IconPath = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);

            CreateTable(
                "dbo.ActivityCategorySet",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ActivityCategoryId = c.Guid(nullable: false),
                        ActivityTemplateId = c.Guid(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityCategory", t => t.ActivityCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.ActivityTemplate", t => t.ActivityTemplateId, cascadeDelete: true)
                .Index(t => t.ActivityCategoryId)
                .Index(t => t.ActivityTemplateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActivityCategorySet", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropForeignKey("dbo.ActivityCategorySet", "ActivityCategoryId", "dbo.ActivityCategory");
            DropIndex("dbo.ActivityCategorySet", new[] { "ActivityTemplateId" });
            DropIndex("dbo.ActivityCategorySet", new[] { "ActivityCategoryId" });
            DropTable("dbo.ActivityCategorySet");
            DropIndex("dbo.ActivityCategory", "Name");
            DropTable("dbo.ActivityCategory");
        }
    }
}
