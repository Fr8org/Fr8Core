namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDocksignAccount : DbMigration
    {
        public override void Up()
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
            
            AddColumn("dbo.Users", "DocusignAccountId", c => c.Int());
            CreateIndex("dbo.Users", "DocusignAccountId");
            AddForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "DocusignAccountId", "dbo.DocusignAccounts");
            DropIndex("dbo.Users", new[] { "DocusignAccountId" });
            DropColumn("dbo.Users", "DocusignAccountId");
            DropTable("dbo.DocusignAccounts");
        }
    }
}
