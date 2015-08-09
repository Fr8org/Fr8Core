namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Add_ActivityStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "State", c => c.String());
            Sql("Update Events Set State=Status");
            DropColumn("dbo.Events", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "Status", c => c.String());
            DropColumn("dbo.Events", "State");
        }
    }
}
