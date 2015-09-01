namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStartingProcessNodeTemplate : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ProcessTemplates", "StartingProcessNodeTemplateId");
            AddForeignKey("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.ProcessTemplates", new[] { "StartingProcessNodeTemplateId" });
        }
    }
}
