namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeFactObjectIdType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Facts", "ObjectId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Facts", "ObjectId", c => c.Int(nullable: false));
        }
    }
}
