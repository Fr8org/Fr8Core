namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class MakeCreatedByRequired : DbMigration
    {
        public override void Up()
        {
                Sql(@"
DECLARE @firstUser VARCHAR(MAX)
SET @firstUser = (select top 1 id from Users)

update Events
set CreatedByID = @firstUser
where CreatedByID IS null");

            DropIndex("dbo.Events", new[] { "CreatedByID" });
            AlterColumn("dbo.Events", "CreatedByID", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Events", "CreatedByID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Events", new[] { "CreatedByID" });
            AlterColumn("dbo.Events", "CreatedByID", c => c.String(maxLength: 128));
            CreateIndex("dbo.Events", "CreatedByID");
        }
    }
}
