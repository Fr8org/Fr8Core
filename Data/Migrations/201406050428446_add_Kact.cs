namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class add_Kact : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Kacts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PrimaryCategory = c.String(),
                        SecondaryCategory = c.String(),
                        Activity = c.String(),
                        ObjectId = c.Int(nullable: false),
                        TaskId = c.Int(nullable: false),
                        CustomerId = c.String(),
                        BookerId = c.Int(nullable: false),
                        AdminId = c.Int(nullable: false),
                        Data = c.String(),
                        Status = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Kacts");
        }
    }
}
