namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Tag_And_FileTags : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Value = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FileTags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagId = c.Int(nullable: false),
                        FileDoId = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Files", t => t.FileDoId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.TagId)
                .Index(t => t.FileDoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.FileTags", "FileDoId", "dbo.Files");
            DropIndex("dbo.FileTags", new[] { "FileDoId" });
            DropIndex("dbo.FileTags", new[] { "TagId" });
            DropTable("dbo.FileTags");
            DropTable("dbo.Tags");
        }
    }
}
