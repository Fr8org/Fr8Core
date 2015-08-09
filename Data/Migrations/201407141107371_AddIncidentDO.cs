namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddIncidentDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Incidents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreateTime = c.DateTimeOffset(nullable: false, precision: 7),
                        PrimaryCategory = c.String(),
                        SecondaryCategory = c.String(),
                        Activity = c.String(),
                        Priority = c.Int(nullable: false),
                        Notes = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Incidents");
        }
    }
}
