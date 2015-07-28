namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessTemplateDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProcessTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        UserId = c.String(),
                        ProcessState = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ProcessTemplateStateTemplate", t => t.ProcessState, cascadeDelete: true)
                .Index(t => t.ProcessState);
            
            CreateTable(
                "dbo._ProcessTemplateStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessTemplates", "ProcessState", "dbo._ProcessTemplateStateTemplate");
            DropIndex("dbo.ProcessTemplates", new[] { "ProcessState" });
            DropTable("dbo._ProcessTemplateStateTemplate");
            DropTable("dbo.ProcessTemplates");
        }
    }
}
