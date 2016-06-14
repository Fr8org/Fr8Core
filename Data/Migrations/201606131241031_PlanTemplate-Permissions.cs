namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanTemplatePermissions : DbMigration
    {
        public override void Up()
        {
            Sql(@"insert into[dbo].[PermissionSets] (
                    [Id], [Name], [ObjectType], [HasFullAccess], [CreateDate], [LastUpdated])
                values(
	                '239E09B3-32E7-4687-B2AC-D8639F8ECE1B',
	                'OwnerOfCurrentObject',
	                'Plan Template',
	                0,
                    getdate(),
                    getdate()
                )");

            Sql(@"insert into[dbo].[RolePermissions] (
                    [Id], [PermissionSetId], [RoleId], [CreateDate], [LastUpdated])
                values(
	                '77E2E1C1-A5F0-48C5-BED3-9480C540F947',
	                '239E09B3-32E7-4687-B2AC-D8639F8ECE1B',
	                (select [Id] from [dbo].[AspNetRoles] where [Name] = 'OwnerOfCurrentObject'),
                    getdate(),
                    getdate()
                )");

            Sql(@"insert into[dbo].[PermissionSetPermissions] ([PermissionSetId], [PermissionTypeTemplateId]) values('239E09B3-32E7-4687-B2AC-D8639F8ECE1B', 1)");
            Sql(@"insert into[dbo].[PermissionSetPermissions] ([PermissionSetId], [PermissionTypeTemplateId]) values('239E09B3-32E7-4687-B2AC-D8639F8ECE1B', 2)");
            Sql(@"insert into[dbo].[PermissionSetPermissions] ([PermissionSetId], [PermissionTypeTemplateId]) values('239E09B3-32E7-4687-B2AC-D8639F8ECE1B', 3)");
            Sql(@"insert into[dbo].[PermissionSetPermissions] ([PermissionSetId], [PermissionTypeTemplateId]) values('239E09B3-32E7-4687-B2AC-D8639F8ECE1B', 4)");
            Sql(@"insert into[dbo].[PermissionSetPermissions] ([PermissionSetId], [PermissionTypeTemplateId]) values('239E09B3-32E7-4687-B2AC-D8639F8ECE1B', 5)");

            Sql(@"insert into[dbo].[ObjectRolePermissions] (
	                [Id],
	                [ObjectId],
	                [RolePermissionId],
	                [Type],
	                [CreateDate],
	                [LastUpdated],
	                [Fr8AccountId]
                )
                select
                    newid(),
                    convert(nvarchar(128), [md].[Id]),
	                '77E2E1C1-A5F0-48C5-BED3-9480C540F947',
	                'Plan Template',
                    getdate(),
                    getdate(),
	                [md].[fr8AccountId]
                from [dbo].[MtData] [md]
                inner join[dbo].[MtTypes] [mt]
                    on [mt].[Id] = [md].[Type]
                where [mt].[Alias] = 'Plan Template'");
        }
        
        public override void Down()
        {
            Sql(@"delete from [dbo].[ObjectRolePermissions] where [RolePermissionId] = '77E2E1C1-A5F0-48C5-BED3-9480C540F947'");
            Sql(@"delete from [dbo].[PermissionSetPermissions] where [PermissionSetId] = '239E09B3-32E7-4687-B2AC-D8639F8ECE1B'");
            Sql(@"delete from [dbo].[RolePermissions] where [PermissionSetId] = '239E09B3-32E7-4687-B2AC-D8639F8ECE1B'");
            Sql(@"delete from [dbo].[PermissionSets] where [Id] = '239E09B3-32E7-4687-B2AC-D8639F8ECE1B'");
        }
    }
}
