namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeRoleName_Customer_to_StandardUser : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("update dbo.AspNetRoles set Name = 'StandardUser' where Name = 'Customer' ");
        }
        
        public override void Down()
        {
        }
    }
}
