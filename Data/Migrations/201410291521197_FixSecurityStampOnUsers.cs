namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixSecurityStampOnUsers : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE AspNetUsers SET SecurityStamp = newid() where SecurityStamp IS NULL");
        }
        
        public override void Down()
        {
        }
    }
}
