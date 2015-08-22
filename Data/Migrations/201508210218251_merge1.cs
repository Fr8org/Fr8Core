namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merge1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts");
            DropIndex("dbo.Users", new[] { "DocusignAccountId" });
          
        }
        
        public override void Down()
        {
           
            CreateIndex("dbo.Users", "DocusignAccountId");
            AddForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts", "Id");
        }
    }
}
