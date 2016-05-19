namespace Data.Repositories.MultiTenant.Sql
{
    partial class SqlMtObjectsStorage
    {
        public const string MtSelectQueryTemplate =
            @"select {0} from (
	            select distinct [md].*
	            from [MtData] [md]
	            inner join [AspNetUserRoles] [anur]
		            on [anur].[UserId] = @account1
	            inner join [MtTypes] [mt]
		            on [mt].[Id] = [md].[Type]
	            left join [RolePermissions] [rp]
		            on [rp].[RoleId] = [anur].[RoleId]
	            left join [PermissionSets] [ps]
		            on [ps].[ObjectType] = [mt].[Alias]
			            and [ps].[Id] = [rp].[PermissionSetId]
	            left join [PermissionSetPermissions] [psp]
		            on [psp].[PermissionSetId] = [ps].[Id]
			            and [psp].[PermissionTypeTemplateId] in (2, 6)
                -- Get permissions for specific record.
	            left join [ObjectRolePermissions] [orp]
		            on [orp].[ObjectId] = convert(nvarchar(128), [md].[Id])
			            and [orp].[RolePermissionId] = [rp].[Id]
			            and [orp].[Type] = [mt].[Alias]
                -- Additional check to see if specific ObjectType and RolePermissionId exists in record-permission table.
	            left join (select distinct [RolePermissionId], [Type] from [ObjectRolePermissions]) [orpagg]
		            on [orpagg].[RolePermissionId] = [rp].[Id]
			            and [orpagg].[Type] = [mt].[Alias]
	            where
		            [md].[Type] = @type
		            and [md].[IsDeleted] = @isDeleted
		            and (
			            [md].[fr8AccountId] = @account2
			            or (
				            [orpagg].[RolePermissionId] is null
				            and [psp].[PermissionTypeTemplateId] is not null
			            )
			            or (
				            [orpagg].[RolePermissionId] is not null
				            and [orp].[ObjectId] is not null
				            and [psp].[PermissionTypeTemplateId] is not null
			            )
		            )
            ) [r]";
    }
}
