namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDockyardDbContextOnModelCreating : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            AddForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            AddForeignKey("dbo.ActionLists", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
