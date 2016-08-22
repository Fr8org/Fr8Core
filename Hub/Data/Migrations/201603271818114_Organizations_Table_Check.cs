namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Organizations_Table_Check : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Organizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "OrganizationId", c => c.Int());
            AddColumn("dbo.AspNetUserClaims", "LastUpdated", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserClaims", "CreateDate", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserClaims", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Users", "OrganizationId");
            AddForeignKey("dbo.Users", "OrganizationId", "dbo.Organizations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Users", new[] { "OrganizationId" });
            DropColumn("dbo.AspNetUserClaims", "Discriminator");
            DropColumn("dbo.AspNetUserClaims", "CreateDate");
            DropColumn("dbo.AspNetUserClaims", "LastUpdated");
            DropColumn("dbo.Users", "OrganizationId");
            DropTable("dbo.Organizations");
        }
    }
}
