namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessTemplate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessTemplates", "ProcessState", "dbo._ProcessStateTemplate");
            CreateTable(
                "dbo._ProcessTemplateStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddForeignKey("dbo.ProcessTemplates", "ProcessState", "dbo._ProcessTemplateStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessTemplates", "ProcessState", "dbo._ProcessTemplateStateTemplate");
            DropTable("dbo._ProcessTemplateStateTemplate");
            AddForeignKey("dbo.ProcessTemplates", "ProcessState", "dbo._ProcessStateTemplate", "Id", cascadeDelete: true);
        }
    }
}
