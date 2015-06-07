namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOldDateCreatedFields : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE [dbo].[Emails] SET [CreateDate] = [DateCreated] WHERE [DateCreated] <> '0001-01-01' AND [CreateDate] = '0001-01-01'");
            Sql("UPDATE [dbo].[Events] SET [CreateDate] = [DateCreated] WHERE [DateCreated] <> '0001-01-01' AND [CreateDate] = '0001-01-01'");
            Sql("UPDATE [dbo].[Negotiations] SET [CreateDate] = [DateCreated] WHERE [DateCreated] <> '0001-01-01' AND [CreateDate] = '0001-01-01'");
            DropColumn("dbo.Emails", "DateCreated");
            DropColumn("dbo.Events", "DateCreated");
            DropColumn("dbo.Negotiations", "DateCreated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Negotiations", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Events", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Emails", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
    }
}
