namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMultiTenantObjects : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MT_Fields",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 150),
                        Type = c.Int(nullable: false),
                        FieldColumnOffset = c.Int(nullable: false),
                        MtObjectId = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Objects", t => t.MtObjectId, cascadeDelete: true)
                .Index(t => new { t.MtObjectId, t.Name, t.FieldColumnOffset }, unique: true, name: "IX_Object_FieldName_Offset");
            
            CreateTable(
                "dbo.MT_Objects",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false),
                        MtOrganizationId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Organizations", t => t.MtOrganizationId, cascadeDelete: true)
                .Index(t => t.MtOrganizationId);
            
            CreateTable(
                "dbo.MT_Organizations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Data",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GUID = c.String(nullable: false),
                        Name = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        MtObjectId = c.String(nullable: false, maxLength: 100),
                        Value1 = c.String(),
                        Value2 = c.String(),
                        Value3 = c.String(),
                        Value4 = c.String(),
                        Value5 = c.String(),
                        Value6 = c.String(),
                        Value7 = c.String(),
                        Value8 = c.String(),
                        Value9 = c.String(),
                        Value10 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Objects", t => t.MtObjectId, cascadeDelete: true)
                .Index(t => t.MtObjectId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MT_Data", "MtObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Fields", "MtObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Objects", "MtOrganizationId", "dbo.MT_Organizations");
            DropIndex("dbo.MT_Data", new[] { "MtObjectId" });
            DropIndex("dbo.MT_Objects", new[] { "MtOrganizationId" });
            DropIndex("dbo.MT_Fields", "IX_Object_FieldName_Offset");
            DropTable("dbo.MT_Data");
            DropTable("dbo.MT_Organizations");
            DropTable("dbo.MT_Objects");
            DropTable("dbo.MT_Fields");
        }
    }
}
