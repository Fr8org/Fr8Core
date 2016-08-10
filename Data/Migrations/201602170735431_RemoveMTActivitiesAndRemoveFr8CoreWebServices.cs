namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMTActivitiesAndRemoveFr8CoreWebServices : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"
              DECLARE @QueryMTDatabase INT = (SELECT TOP 1 a.Id FROM ActivityTemplate a WHERE a.Name = 'QueryMTDatabase')
              DECLARE @StoreMTDataId INT = (SELECT TOP 1 a.Id FROM ActivityTemplate a WHERE a.Name = 'StoreMTData')
              DECLARE @frCore INT = (SELECT TOP 1 a.Id FROM WebServices a WHERE a.Name = 'fr8 Core')

              --Delete Actions 
              DELETE FROM Actions
              WHERE ActivityTemplateId IN (@QueryMTDatabase, @StoreMTDataId)

              --Delete Activity
              DELETE FROM ActivityTemplate
              WHERE Id IN (@QueryMTDatabase, @StoreMTDataId)");
        }
        
        public override void Down()
        {
        }
    }
}
