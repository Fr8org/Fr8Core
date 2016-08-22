namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Drop_Old_Profile_Tables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles");
            DropForeignKey("dbo.Profiles", "DockyardAccountID", "dbo.Users");
            DropIndex("dbo.Profiles", new[] { "DockyardAccountID" });
            DropIndex("dbo.ProfileNodes", new[] { "ProfileID" });
            DropIndex("dbo.ProfileNodes", new[] { "ParentNodeID" });
            DropIndex("dbo.ProfileItems", new[] { "ProfileNodeID" });
            DropTable("dbo.Profiles");
            DropTable("dbo.ProfileNodes");
            DropTable("dbo.ProfileItems");
            DropTable("dbo.ProfileNodeAncestorsCTEView");
            DropTable("dbo.ProfileNodeDescendantsCTEView");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProfileNodeDescendantsCTEView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        ProfileParentNodeID = c.Int(),
                        AnchorNodeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileNodeAncestorsCTEView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        ProfileParentNodeID = c.Int(),
                        AnchorNodeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        Key = c.String(),
                        Value = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProfileID = c.Int(nullable: false),
                        ParentNodeID = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DockyardAccountID = c.String(maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ProfileItems", "ProfileNodeID");
            CreateIndex("dbo.ProfileNodes", "ParentNodeID");
            CreateIndex("dbo.ProfileNodes", "ProfileID");
            CreateIndex("dbo.Profiles", "DockyardAccountID");
            AddForeignKey("dbo.Profiles", "DockyardAccountID", "dbo.Users", "Id");
            AddForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes", "Id");
        }
    }
}
