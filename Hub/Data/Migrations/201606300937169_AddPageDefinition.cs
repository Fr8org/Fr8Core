namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPageDefinition : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PageDefinitions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Tags = c.String(nullable: false),
                        Url = c.String(),
                        Type = c.String(nullable: false),
                        Description = c.String(),
                        PageName = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PageDefinitions");
        }
    }
}
