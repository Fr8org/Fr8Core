namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFr8CoreActivities : DbMigration
    {
        public override void Up()
        {
            // Deleting Fr8 Core from Actions
            const string SubSqlScript = "SELECT at.Id FROM [dbo].[ActivityTemplate] AS at JOIN [dbo].[WebServices] as ws ON at.WebServiceId = ws.Id WHERE ws.Name LIKE 'fr8 Core'";
            Sql("DELETE FROM [dbo].[Actions] WHERE ActivityTemplateId IN ( " + SubSqlScript + ")");
            // Deleting Fr8 Core from ActivityTemplate
            Sql("DELETE FROM [dbo].[ActivityTemplate] WHERE Id IN ( " + SubSqlScript + ")");
            // Deleting Fr8 Core from WebServices
            Sql("DELETE FROM [dbo].[WebServices] WHERE Name LIKE 'fr8 Core'");
        }
        
        public override void Down()
        {
        }
    }
}
