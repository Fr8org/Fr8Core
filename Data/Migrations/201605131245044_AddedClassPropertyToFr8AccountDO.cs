namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedClassPropertyToFr8AccountDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Class", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Class");
        }
    }
}
