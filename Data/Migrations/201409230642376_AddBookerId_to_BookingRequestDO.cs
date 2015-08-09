namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddBookerId_to_BookingRequestDO : DbMigration
    {
        public override void Up()
        {
            //Drop the column if it already exists.. since the migrations were corrupted
            Sql(@"
if exists(select * from sys.columns 
            where Name = N'BookerID' and Object_ID = Object_ID(N'BookingRequests'))
begin
    ALTER TABLE dbo.BookingRequests DROP COLUMN BookerID
end
");
            AddColumn("dbo.BookingRequests", "BookerID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookingRequests", "BookerID");
        }
    }
}
