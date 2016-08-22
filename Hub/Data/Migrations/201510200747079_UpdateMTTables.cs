namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMTTables : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MT_Objects", "MT_OrganizationId", "dbo.MT_Organizations");
            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Objects", new[] { "MT_OrganizationId" });
            AddColumn("dbo.MT_Objects", "ManifestId", c => c.Int(nullable: false));
            AlterColumn("dbo.MT_Fields", "MT_FieldType_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.MT_Data", "fr8AccountId", c => c.String(nullable: false));
            CreateIndex("dbo.MT_Fields", "MT_FieldType_Id");
            AddForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType", "Id", cascadeDelete: true);
            DropColumn("dbo.MT_Objects", "MT_OrganizationId");
            DropColumn("dbo.MT_Data", "Name");
            DropTable("dbo.MT_Organizations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MT_Organizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.MT_Data", "Name", c => c.String(nullable: false));
            AddColumn("dbo.MT_Objects", "MT_OrganizationId", c => c.Int(nullable: false));
            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            AlterColumn("dbo.MT_Data", "fr8AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.MT_Fields", "MT_FieldType_Id", c => c.Int());
            DropColumn("dbo.MT_Objects", "ManifestId");
            CreateIndex("dbo.MT_Objects", "MT_OrganizationId");
            CreateIndex("dbo.MT_Fields", "MT_FieldType_Id");
            AddForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType", "Id");
            AddForeignKey("dbo.MT_Objects", "MT_OrganizationId", "dbo.MT_Organizations", "Id", cascadeDelete: true);
        }
    }
}
