namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorHistory : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Facts", newName: "History");
            AddColumn("dbo.History", "Priority", c => c.Int());
            AddColumn("dbo.History", "Notes", c => c.String());
            AddColumn("dbo.History", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.History", "Name");
            DropColumn("dbo.History", "TaskId");
            DropColumn("dbo.History", "AdminId");
            Sql("UPDATE dbo.Logs SET [CreateDate] = [Date] WHERE [CreateDate] = '0001-01-01'");
            DropColumn("dbo.Logs", "Date");
            
            Sql("UPDATE dbo.History SET Discriminator = 'FactDO'");
            Sql(
@"INSERT INTO [dbo].[History] (
       [Discriminator]
	  ,[PrimaryCategory]
      ,[SecondaryCategory]
      ,[Activity]
      ,[Priority]
      ,[Notes]
      ,[ObjectId]
      ,[CustomerId]
      ,[BookerId]
      ,[LastUpdated]
      ,[CreateDate]
      ,[Data]
      ,[Status])
SELECT 'IncidentDO'
      ,[PrimaryCategory]
      ,[SecondaryCategory]
      ,[Activity]
      ,[Priority]
      ,[Notes]
      ,[ObjectId]
      ,[CustomerId]
      ,[BookerId]
      ,[LastUpdated]
      ,[CreateDate]
      ,[Data]
      ,[Status]
  FROM [dbo].[Incidents]"); 
            
            DropTable("dbo.Incidents");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Incidents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PrimaryCategory = c.String(),
                        SecondaryCategory = c.String(),
                        Activity = c.String(),
                        Priority = c.Int(nullable: false),
                        Notes = c.String(),
                        ObjectId = c.Int(nullable: false),
                        CustomerId = c.String(),
                        BookerId = c.String(),
                        Data = c.String(),
                        Status = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);

            Sql(
@"INSERT INTO [dbo].[Incidents] (
       [PrimaryCategory]
      ,[SecondaryCategory]
      ,[Activity]
      ,[Priority]
      ,[Notes]
      ,[ObjectId]
      ,[CustomerId]
      ,[BookerId]
      ,[LastUpdated]
      ,[CreateDate]
      ,[Data]
      ,[Status])
SELECT [PrimaryCategory]
      ,[SecondaryCategory]
      ,[Activity]
      ,[Priority]
      ,[Notes]
      ,[ObjectId]
      ,[CustomerId]
      ,[BookerId]
      ,[LastUpdated]
      ,[CreateDate]
      ,[Data]
      ,[Status]
  FROM [dbo].[History]
  WHERE [Discriminator] = 'IncidentDO'");
            Sql("DELETE [dbo].[History] WHERE [Discriminator] = 'IncidentDO'");
            AddColumn("dbo.Logs", "Date", c => c.DateTime(nullable: false));
            Sql("UPDATE dbo.Logs SET [Date] = [CreateDate]");
            AddColumn("dbo.History", "AdminId", c => c.Int(nullable: false));
            AddColumn("dbo.History", "TaskId", c => c.Int(nullable: false));
            AddColumn("dbo.History", "Name", c => c.String());
            DropColumn("dbo.History", "Discriminator");
            DropColumn("dbo.History", "Notes");
            DropColumn("dbo.History", "Priority");
            RenameTable(name: "dbo.History", newName: "Facts");
        }
    }
}
