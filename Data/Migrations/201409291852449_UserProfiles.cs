namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserProfiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.ProfileNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileID = c.Int(nullable: false),
                        ParentNodeID = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfileNodes", t => t.ParentNodeID)
                .ForeignKey("dbo.Profiles", t => t.ProfileID, cascadeDelete: true)
                .Index(t => t.ProfileID)
                .Index(t => t.ParentNodeID);
            
            CreateTable(
                "dbo.ProfileItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        Key = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfileNodes", t => t.ProfileNodeID, cascadeDelete: true)
                .Index(t => t.ProfileNodeID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Profiles", "UserID", "dbo.Users");
            DropForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles");
            DropForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes");
            DropIndex("dbo.ProfileItems", new[] { "ProfileNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ParentNodeID" });
            DropIndex("dbo.ProfileNodes", new[] { "ProfileID" });
            DropIndex("dbo.Profiles", new[] { "UserID" });
            DropTable("dbo.ProfileItems");
            DropTable("dbo.ProfileNodes");
            DropTable("dbo.Profiles");
        }
    }
}
