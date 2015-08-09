namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class change_datetime_to_offset_Fact : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Facts", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Facts", "CreateDate", c => c.DateTime(nullable: false));
        }
    }
}
