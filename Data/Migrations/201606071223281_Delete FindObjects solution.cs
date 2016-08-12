namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteFindObjectssolution : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            // according to FR-3733 and FR-3574 we should delete old Activity templates, Alp delet his migration so i transfer his code here and commented out mein 
            // because it`s already will be done with his queries.

            //var query = //"DELETE FROM Actions WHERE ActivityTemplateId IN (SELECT id FROM ActivityTemplate WHERE name LIKE 'FindObjects_Solution' );" +
            //            //"DELETE FROM ActivityTemplate WHERE name LIKE 'FindObjects_Solution';" +
            //            "UPDATE ActivityTemplate SET ActivityTemplateState = 0 WHERE name LIKE 'FindObjects_Solution';";

            const string SubSqlScript = "SELECT at.Id FROM [dbo].[ActivityTemplate] AS at JOIN [dbo].[WebServices] as ws ON at.WebServiceId = ws.Id WHERE ws.Name LIKE 'fr8 Core'";
            
            //comment this because of DELETE constraints 
            //Sql("DELETE FROM [dbo].[Actions] WHERE ActivityTemplateId IN ( " + SubSqlScript + ")");
            
            // Deleting Fr8 Core from ActivityTemplate
            Sql("UPDATE [dbo].[ActivityTemplate] SET ActivityTemplateState = 0  WHERE Id IN ( " + SubSqlScript + ")");
            
            // Deleting Fr8 Core from WebServices
            // Sql("DELETE FROM [dbo].[WebServices] WHERE Name LIKE 'fr8 Core'");

            //Sql(query);
        }
        
        public override void Down()
        {

            const string SubSqlScript = "SELECT at.Id FROM [dbo].[ActivityTemplate] AS at JOIN [dbo].[WebServices] as ws ON at.WebServiceId = ws.Id WHERE ws.Name LIKE 'fr8 Core'";
            Sql("UPDATE [dbo].[ActivityTemplate] SET ActivityTemplateState = 1  WHERE Id IN ( " + SubSqlScript + ")");

            //var query = "UPDATE ActivityTemplate SET ActivityTemplateState = 1 WHERE name LIKE 'FindObjects_Solution';";
            //@"INSERT INTO ActivityTemplate
            //      ([Id]
            //      ,[Name]
            //      ,[Label]
            //      ,[Tags]
            //      ,[Version]
            //      ,[Description]
            //      ,[NeedsAuthentication]
            //      ,[ComponentActivities]
            //      ,[ActivityTemplateState]
            //      ,[TerminalId]
            //      ,[Category]
            //      ,[Type]
            //      ,[MinPaneWidth]
            //      ,[WebServiceId]
            //      ,[ClientVisibility]
            //      ,[LastUpdated]
            //      ,[CreateDate])
            //VALUES
            //      ('2F4F50FF-13A0-492C-B0B3-4D0C8B4119F4'
            //      ,'FindObjects_Solution'
            //      ,'Find Objects Solution'
            //      ,NULL
            //      ,1
            //      ,NULL
            //      ,0
            //      ,NULL
            //      ,0
            //      ,3
            //      ,5
            //      ,3
            //      ,330
            //      ,6
            //      ,1
            //      ,'0001-01-01 00:00:00.0000000 +00:00'
            //      ,'0001-01-01 00:00:00.0000000 +00:00');";

            // Sql(query);
        }
    }
}
