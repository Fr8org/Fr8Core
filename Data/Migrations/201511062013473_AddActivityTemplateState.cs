namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Data.States;

    public partial class AddActivityTemplateState : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._ActivityTemplateStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActivityTemplate", "ActivityTemplateState", c => c.Int(nullable: true));
            SeedConstants<ActivityTemplateState>("dbo._ActivityTemplateStateTemplate");

            Sql(string.Format("UPDATE dbo.ActivityTemplate SET ActivityTemplateState = {0} WHERE ActivityTemplateState IS NULL", ActivityTemplateState.Active));

            AlterColumn("dbo.ActivityTemplate", "ActivityTemplateState", c => c.Int(nullable: false));
            CreateIndex("dbo.ActivityTemplate", "ActivityTemplateState");
            AddForeignKey("dbo.ActivityTemplate", "ActivityTemplateState", "dbo._ActivityTemplateStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActivityTemplate", "ActivityTemplateState", "dbo._ActivityTemplateStateTemplate");
            DropIndex("dbo.ActivityTemplate", new[] { "ActivityTemplateState" });
            DropColumn("dbo.ActivityTemplate", "ActivityTemplateState");
            DropTable("dbo._ActivityTemplateStateTemplate");
        }
    }
}
