namespace Data.Migrations
{
    public partial class RemoveInactualAdminUsers : DbMigration
    {
        public override void Up()
        {
            Sql(
@"DELETE FROM dbo.AspNetUserRoles 
WHERE 
RoleId = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Admin')
AND UserId IN (SELECT Id FROM AspNetUsers WHERE UserName IN ('mvcdeveloper@gmail.com', 'd1984v@gmail.com', 'farrukh.normuradov@gmail.com'))");
        }
        
        public override void Down()
        {
            //This migration has no meaningful revert logic
        }
    }
}
