namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeDocusignAccountDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts");
            DropIndex("dbo.Users", new[] { "DocusignAccountId" });
            DropTable("dbo.DocusignAccounts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DocusignAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Users", "DocusignAccountId");
            AddForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts", "Id");
        }
    }
}
