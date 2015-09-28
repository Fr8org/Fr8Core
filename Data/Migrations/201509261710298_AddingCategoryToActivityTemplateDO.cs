using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingCategoryToActivityTemplateDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "Category", c => c.Int(nullable: false));
            // At the time of writing migration ActivityTemplate service was unable to update ActivityTemplates. 
            // To reflect changes caused by adding Category field we have to maualy update the table with correct values. 
            // The other possible solution was to delete content of the table, but due to possible side effects we just do the update.
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'Write_To_Sql_Server'", (int)ActivityCategory.fr8_Forwarder));
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'FilterUsingRunTimeData'", (int)ActivityCategory.fr8_Processor));
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'MapFields'", (int)ActivityCategory.fr8_Processor));
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'Wait_For_DocuSign_Event'", (int)ActivityCategory.fr8_Receiver));
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'Extract_From_DocuSign_Envelope'", (int)ActivityCategory.fr8_Receiver));
            Sql(string.Format("Update dbo.ActivityTemplate set Category = {0} where Name = 'Publish_To_Slack'", (int)ActivityCategory.fr8_Forwarder));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Category");
        }
    }
}
