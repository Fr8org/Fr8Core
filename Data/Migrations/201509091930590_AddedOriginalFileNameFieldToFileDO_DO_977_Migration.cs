namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOriginalFileNameFieldToFileDO_DO_977_Migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "OriginalFileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "OriginalFileName");
        }
    }
}
