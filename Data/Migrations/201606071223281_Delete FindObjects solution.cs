namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteFindObjectssolution : DbMigration
    {
        public override void Up()
        {
            var query = //"DELETE FROM Actions WHERE ActivityTemplateId IN (SELECT id FROM ActivityTemplate WHERE name LIKE 'FindObjects_Solution' );" +
                        //"DELETE FROM ActivityTemplate WHERE name LIKE 'FindObjects_Solution';" +
                        "UPDATE ActivityTemplate SET ActivityTemplateState = 0 WHERE name LIKE 'FindObjects_Solution';";

            Sql(query);
        }
        
        public override void Down()
        {
            var query = "UPDATE ActivityTemplate SET ActivityTemplateState = 1 WHERE name LIKE 'FindObjects_Solution';";
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

            Sql(query);
        }
    }
}
