namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSystemAccountIdentifier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "SystemAccount", c => c.Boolean(nullable: false));

            //update for production/dev the current system user
            Sql("update dbo.Users set SystemAccount = 1 where id = (select top 1 Id from dbo.AspNetUsers where UserName like 'system1@fr8.co') ");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "SystemAccount");
        }
    }
}
