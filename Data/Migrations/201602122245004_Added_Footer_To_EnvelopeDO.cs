namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Footer_To_EnvelopeDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Envelopes", "Footer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Envelopes", "Footer");
        }
    }
}
