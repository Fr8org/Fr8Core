namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeAttendeesPKV2 : DbMigration
    {
        public override void Up()
        {
            var tableName = "Attendees";
            var tempName = tableName + "_temp";
            var sql = String.Format(@"

BEGIN TRANSACTION

EXECUTE sp_rename N'[PK_dbo.{0}]', N'[PK_{0}_old]', 'OBJECT'

CREATE TABLE [dbo].[{1}]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[Name] VARCHAR(MAX) NULL,
[EventID] [int] NOT NULL,
[EmailAddressID] [int] NOT NULL
) ON [PRIMARY] 

ALTER TABLE [dbo].[{1}] ADD CONSTRAINT [PK_dbo.{0}] PRIMARY KEY CLUSTERED  ([Id])
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
            var tableName = "Attendees";
            var tempName = tableName + "_temp";
            var sql = String.Format(@"

BEGIN TRANSACTION

EXECUTE sp_rename N'[PK_dbo.{0}]', N'[PK_{0}_old]', 'OBJECT'

CREATE TABLE [dbo].[{1}]
(
[Id] [int] NOT NULL,
[Name] VARCHAR(MAX) NULL,
[EventID] [int] NOT NULL,
[EmailAddressID] [int] NOT NULL
) ON [PRIMARY] 

ALTER TABLE [dbo].[{1}] ADD CONSTRAINT [PK_dbo.{0}] PRIMARY KEY CLUSTERED  ([Id], [EventID])


INSERT INTO [dbo].[{1}] (Id, Name, EventID, EmailAddressID)
SELECT Id, Name, EventID, EmailAddressID FROM dbo.{0}

DROP TABLE dbo.{0}

EXEC sp_rename N'[dbo].[{1}]', N'{0}';

COMMIT TRANSACTION
", tableName, tempName);

            Sql(sql);

        }
    }
}
