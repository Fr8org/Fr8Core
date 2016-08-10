namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedNullableActivityTemplateIdFromActivityDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });
            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });
            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Int());
            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id");
        }
    }
}
