namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActivityActivationState : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "ActivationState", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Actions", "ActivationState");
        }
    }
}
