namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_Save_In_Google_Sheet_Activity : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM dbo.ActivityTemplate WHERE Name like 'Save_In_Google_Sheet'");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM dbo.ActivityTemplate WHERE Name like 'Save_In_Google_Sheet'");
        }
    }
}
