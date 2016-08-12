namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkRolePermissionsToStandardUserRole : DbMigration
    {
        public override void Up()
        {
            Sql(@" declare @newStandardUserRoleId nvarchar(128)
                   declare @oldCustomerRoleId nvarchar(128)
                   declare @oldBookerRoleId nvarchar(128)

                   set @newStandardUserRoleId = (select top 1 id from dbo.AspNetRoles where Name = 'StandardUser')
                   set @oldCustomerRoleId = (select top 1 id from dbo.AspNetRoles where Name = 'Customer')
                   set @oldBookerRoleId = (select top 1 id from dbo.AspNetRoles where Name = 'Booker')

                   update dbo.RolePermissions Set RoleId = @newStandardUserRoleId where RoleId = @oldCustomerRoleId

                   --delete old Customer and Booker roles from db   
                    delete from dbo.AspNetRoles where Id in (@oldCustomerRoleId, @oldBookerRoleId)
                ");
        }
        
        public override void Down()
        {
        }
    }
}
