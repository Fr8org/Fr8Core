namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Salesforce_Activities_Clubbed : DbMigration
    {
        public override void Up()
        {
            Sql(@"DELETE FROM [dbo].[ActivityTemplate] WHERE [Name] = 'Create_Lead' OR [Name] = 'Create_Contact' OR [Name] = 'Create_Account'");
        }
        
        public override void Down()
        {
            Sql(@"DELETE FROM [dbo].[ActivityTemplate] WHERE [Name] = 'Save_To_SalesforceDotCom'");
        }
    }
}
