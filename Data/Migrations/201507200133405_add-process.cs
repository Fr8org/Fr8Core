namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprocess : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProcessDOes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProcessDOes");
        }
    }
}
