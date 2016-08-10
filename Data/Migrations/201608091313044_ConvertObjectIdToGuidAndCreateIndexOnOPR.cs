namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ConvertObjectIdToGuidAndCreateIndexOnOPR : System.Data.Entity.Migrations.DbMigration
    {
        private const string CreateObjectRolePermissionsTableQuery = @"CREATE TABLE [dbo].[ObjectRolePermissions](
	        [Id] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
	        [ObjectId] [uniqueidentifier] NOT NULL,
	        [RolePermissionId] [uniqueidentifier] NOT NULL,
	        [Type] [nvarchar](max) NULL,
	        [PropertyName] [nvarchar](max) NULL,
	        [LastUpdated] [datetimeoffset](7) NOT NULL,
	        [CreateDate] [datetimeoffset](7) NOT NULL,
	        [Fr8AccountId] [nvarchar](128) NOT NULL DEFAULT (''),
	        [OrganizationId] [int] NULL,
         CONSTRAINT [PK_dbo.ObjectRolePermissions_] PRIMARY KEY CLUSTERED 
        (
	        [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

        ALTER TABLE [dbo].[ObjectRolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ObjectRolePermissions_dbo.RolePermissions_RolePermissionId_] FOREIGN KEY([RolePermissionId])
        REFERENCES [dbo].[RolePermissions] ([Id])

        ALTER TABLE [dbo].[ObjectRolePermissions] CHECK CONSTRAINT [FK_dbo.ObjectRolePermissions_dbo.RolePermissions_RolePermissionId_]
        ";
        public override void Up()
        {
            RenameTable("dbo.ObjectRolePermissions", "OldObjectRolePermissions");
            Sql(CreateObjectRolePermissionsTableQuery);
            //copy data from existing table - parse object ids as guid
            Sql(
                @"INSERT INTO [dbo].[ObjectRolePermissions] (
                [Id],
	            [ObjectId],
	            [RolePermissionId],
	            [Type],
	            [PropertyName],
	            [LastUpdated],
	            [CreateDate],
	            [Fr8AccountId],
	            [OrganizationId])
                SELECT
                    [ot].[Id],
	                CAST([ot].[ObjectId] AS UNIQUEIDENTIFIER),
	                [ot].[RolePermissionId],
	                [ot].[Type],
	                [ot].[PropertyName],
	                [ot].[LastUpdated],
	                [ot].[CreateDate],
	                [ot].[Fr8AccountId],
	                [ot].[OrganizationId]
                FROM [dbo].[OldObjectRolePermissions] AS [ot]"
            );
            DropTable("dbo.OldObjectRolePermissions");
            CreateIndex("dbo.ObjectRolePermissions", "ObjectId");
        }

        public override void Down()
        {
            DropIndex("dbo.ObjectRolePermissions", new[] { "ObjectId" });
        }

    }
}
