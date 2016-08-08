namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateIndexForObjectIdOnObjectRolePermissions : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ObjectRolePermissions", "ObjectId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ObjectRolePermissions", new[] { "ObjectId" });
        }
    }
}
