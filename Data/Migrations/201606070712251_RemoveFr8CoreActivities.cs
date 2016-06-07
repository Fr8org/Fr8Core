namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFr8CoreActivities : DbMigration
    {
        public override void Up()
        {
            // Deleting Fr8 Core from Actions
            const string SubSqlScript = "SELECT at.Id FROM [DockyardDB2].[dbo].[ActivityTemplate] AS at JOIN [DockyardDB2].[dbo].[WebServices] as ws ON at.WebServiceId = ws.Id WHERE ws.Name LIKE 'fr8 Core'";
            Sql("DELETE FROM [DockyardDB2].[dbo].[Actions] WHERE ActivityTemplateId IN ( " + SubSqlScript + ")");
            // Deleting Fr8 Core from ActivityTemplate
            Sql("DELETE FROM [DockyardDB2].[dbo].[ActivityTemplate] WHERE Id IN ( " + SubSqlScript + ")");
            // Deleting Fr8 Core from WebServices
            Sql("DELETE FROM [DockyardDB2].[dbo].[WebServices] WHERE Name LIKE 'fr8 Core'");
        }
        
        public override void Down()
        {
        }
    }
}
