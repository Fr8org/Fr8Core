using Data.Repositories;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Terminals_Standard_Permissions : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            // add use terminal permissions for all present terminals for standard users and for guests
            var permissionSetId = Guid.NewGuid();
            var standardUserRolePermissionId = Guid.NewGuid();
            var guestUserRolePermissionId = Guid.NewGuid();

            var sqlMigration = $@" 
                -- add permission type template for terminals
                declare @counter int;
                set @counter = (select count (*) from dbo._PermissionTypeTemplate where Id = 11)
                if(@counter = 0) 
                begin 
	                insert into dbo._PermissionTypeTemplate(id, Name) values(11, 'UseTerminal')
                end

                -- create permissionset for terminals with use permissions
                insert into dbo.PermissionSets(Id, Name,  ObjectType,HasFullAccess, LastUpdated, CreateDate) 
                 values('{permissionSetId}', 'Use Terminal Permission Set', 'TerminalDO', 0, '2016-07-20 11:11:48.5762342 +02:00', '2016-07-20 11:11:48.5762342 +02:00')
                
                insert into dbo.PermissionSetPermissions(PermissionSetId, PermissionTypeTemplateId) values('{permissionSetId}', 11) 

                --check if roles exist for recreating new 
                set @counter = 0;
                set @counter = (select top 1 Id from dbo.AspNetRoles where Name= 'Guest')
                if(@counter = 0) 
                begin 
	                insert into dbo.AspNetRoles(id, Name, LastUpdated, CreateDate, Discriminator) values(newid(), 'Guest','2016-07-20 11:11:48.5762342 +02:00', '2016-07-20 11:11:48.5762342 +02:00', 'AspNetRolesDO' )
                end
                set @counter = 0;
                set @counter = (select top 1 Id from dbo.AspNetRoles where Name= 'StandardUser')
                if(@counter = 0) 
                begin 
	                insert into dbo.AspNetRoles(id, Name, LastUpdated, CreateDate, Discriminator) values(newid(), 'StandardUser','2016-07-20 11:11:48.5762342 +02:00', '2016-07-20 11:11:48.5762342 +02:00', 'AspNetRolesDO' )
                end

                -- create RolePermissions with given permission set             
                insert into dbo.RolePermissions(Id, PermissionSetId, RoleId, LastUpdated, CreateDate) 
                    values ('{guestUserRolePermissionId}', '{permissionSetId}', (select top 1 Id from dbo.AspNetRoles where Name= 'Guest'), '2016-07-20 11:11:48.5762342 +02:00', '2016-07-20 11:11:48.5762342 +02:00')
                insert into dbo.RolePermissions(Id, PermissionSetId, RoleId, LastUpdated, CreateDate) 
                    values ('{standardUserRolePermissionId}', '{permissionSetId}', (select top 1 Id from dbo.AspNetRoles where Name= 'Customer'), '2016-07-20 11:11:48.5762342 +02:00', '2016-07-20 11:11:48.5762342 +02:00')

                -- create ObjectRolePermissions for all terminals for above defined 2 RolePermissions
                insert into dbo.ObjectRolePermissions(id, ObjectId, RolePermissionId, Type, LastUpdated, CreateDate, Fr8AccountId)
                select newid(), id, '{standardUserRolePermissionId}', 'TerminalDO', '2016-07-01 11:11:48.5762342 +02:00','2016-07-01 11:11:48.5762342 +02:00', ' ' from dbo.Terminals

                insert into dbo.ObjectRolePermissions(id, ObjectId, RolePermissionId, Type, LastUpdated, CreateDate, Fr8AccountId)
                select newid(), id, '{guestUserRolePermissionId}', 'TerminalDO', '2016-07-01 11:11:48.5762342 +02:00','2016-07-01 11:11:48.5762342 +02:00', ' ' from dbo.Terminals
                ";

            Sql(sqlMigration);
        }
        
        public override void Down()
        {
        }
    }
}
