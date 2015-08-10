namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixPKs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "Id", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "Id" });

            var tableName = "Attendees";
            var tempName = tableName + "_temp";
            var sql = String.Format(@"

BEGIN TRANSACTION

EXECUTE sp_rename N'[PK_dbo.{0}]', N'[PK_{0}_old]', 'OBJECT'
EXECUTE sp_rename N'[FK_{0}_Events]', N'[FK_{0}_Events_old]', 'OBJECT'
EXECUTE sp_rename N'[FK_{0}_EmailAddresses]', N'[FK_{0}__EmailAddresses_old]', 'OBJECT'

CREATE TABLE [dbo].[{1}]
(
[Id] [int] NOT NULL IDENTITY,
[Name] VARCHAR(MAX) NULL,
[EventID] [int] NOT NULL,
[EmailAddressID] [int] NOT NULL,
CONSTRAINT [PK_dbo.{0}] PRIMARY KEY ([Id])
) ON [PRIMARY] 

ALTER TABLE [dbo].[{1}] ADD CONSTRAINT [FK_{0}_Events] FOREIGN KEY ([EventID]) REFERENCES [dbo].[Events] ([Id])
ALTER TABLE [dbo].[{1}] ADD CONSTRAINT [FK_{0}_EmailAddresses] FOREIGN KEY ([EmailAddressID]) REFERENCES [dbo].[EmailAddresses] ([Id])

SET IDENTITY_INSERT [dbo].{1} ON;
INSERT INTO [dbo].[{1}] (Id, Name, EventID, EmailAddressID)
SELECT Id, Name, EventID, EmailAddressID FROM dbo.{0}
SET IDENTITY_INSERT [dbo].{1} OFF;

DROP TABLE dbo.{0}

EXEC sp_rename N'[dbo].[{1}]', N'{0}';

COMMIT TRANSACTION
", tableName, tempName);

            Sql(sql);

        }
        
        public override void Down()
        {
            //There's no going back! If you want to revert this, you'll have to do it manually. Too difficult to support azure's SqlServer and our local ones.
        }
    }
}
