namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActivityTemplateDO_Id_Int32_To_Guid : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });
            DropPrimaryKey("dbo.ActivityTemplate");
            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Guid(nullable: false));
            AlterColumn("dbo.ActivityTemplate", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.ActivityTemplate", "Id");
            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate");
            DropIndex("dbo.Actions", new[] { "ActivityTemplateId" });
            DropPrimaryKey("dbo.ActivityTemplate");
            AlterColumn("dbo.ActivityTemplate", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Actions", "ActivityTemplateId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.ActivityTemplate", "Id");
            CreateIndex("dbo.Actions", "ActivityTemplateId");
            AddForeignKey("dbo.Actions", "ActivityTemplateId", "dbo.ActivityTemplate", "Id", cascadeDelete: true);
        }
    }
}
