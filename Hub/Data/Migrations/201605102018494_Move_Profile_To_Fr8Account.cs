namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Move_Profile_To_Fr8Account : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.AspNetRoles", new[] { "ProfileId" });
            AddColumn("dbo.Profiles", "Protected", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "ProfileId", c => c.Guid());
            CreateIndex("dbo.Users", "ProfileId");
            AddForeignKey("dbo.Users", "ProfileId", "dbo.Profiles", "Id");
            DropColumn("dbo.AspNetRoles", "ProfileId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetRoles", "ProfileId", c => c.Guid());
            DropForeignKey("dbo.Users", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.Users", new[] { "ProfileId" });
            DropColumn("dbo.Users", "ProfileId");
            DropColumn("dbo.Profiles", "Protected");
            CreateIndex("dbo.AspNetRoles", "ProfileId");
            AddForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles", "Id");
        }
    }
}
