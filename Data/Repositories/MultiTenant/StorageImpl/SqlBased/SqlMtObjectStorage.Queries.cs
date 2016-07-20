namespace Data.Repositories.MultiTenant.Sql
{
    partial class SqlMtObjectsStorage
    {
        public const string MtSelectQueryTemplate =
            @"SELECT {0} FROM (select {1}, ROW_NUMBER() OVER (PARTITION BY [md].Id order by [md].Id) AS RowNumber
	            from [MtData] [md]
	            inner join [MtTypes] [mt]
		            on [mt].[Id] = [md].[Type]
	            left join [AspNetUserRoles] [anur]
		            on [anur].[UserId] = @account
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
	            where
		            [md].[Type] = @type
		            and [md].[IsDeleted] = @isDeleted
		            and (
			            [md].[fr8AccountId] = @account
			            or (
				            [orp].[ObjectId] is not null
				            and [psp].[PermissionTypeTemplateId] is not null
			            ) 
		            ) {2}) as tmp where tmp.RowNumber = 1";


        public const string MtSelectScalarTemplate =
            @"select count(distinct [md].Id) 
	            from [MtData] [md]
	            inner join [MtTypes] [mt]
		            on [mt].[Id] = [md].[Type]
	            left join [AspNetUserRoles] [anur]
		            on [anur].[UserId] = @account
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
	            where
		            [md].[Type] = @type
		            and [md].[IsDeleted] = @isDeleted
		            and (
			            [md].[fr8AccountId] = @account
			            or (
				            [orp].[ObjectId] is not null
				            and [psp].[PermissionTypeTemplateId] is not null
			            )
		            ) {0}";
    }
}
