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
                        Id = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 150),
                        Type = c.Int(nullable: false),
                        FieldColumnOffset = c.Int(nullable: false),
                        MT_ObjectId = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Objects", t => t.MT_ObjectId, cascadeDelete: true)
                .Index(t => new { t.MT_ObjectId, t.Name, t.FieldColumnOffset }, name: "FieldColumnOffsetIndex");
            
            CreateTable(
                "dbo.MT_Objects",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false),
                        MT_OrganizationId = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Organizations", t => t.MT_OrganizationId, cascadeDelete: true)
                .Index(t => t.MT_OrganizationId);
            
            CreateTable(
                "dbo.MT_Organizations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Data",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        GUID = c.Guid(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        MT_ObjectId = c.String(nullable: false, maxLength: 100),
                        IsDeleted = c.Boolean(nullable: false),
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
                        Value11 = c.String(),
                        Value12 = c.String(),
                        Value13 = c.String(),
                        Value14 = c.String(),
                        Value15 = c.String(),
                        Value16 = c.String(),
                        Value17 = c.String(),
                        Value18 = c.String(),
                        Value19 = c.String(),
                        Value20 = c.String(),
                        Value21 = c.String(),
                        Value22 = c.String(),
                        Value23 = c.String(),
                        Value24 = c.String(),
                        Value25 = c.String(),
                        Value26 = c.String(),
                        Value27 = c.String(),
                        Value28 = c.String(),
                        Value29 = c.String(),
                        Value30 = c.String(),
                        Value31 = c.String(),
                        Value32 = c.String(),
                        Value33 = c.String(),
                        Value34 = c.String(),
                        Value35 = c.String(),
                        Value36 = c.String(),
                        Value37 = c.String(),
                        Value38 = c.String(),
                        Value39 = c.String(),
                        Value40 = c.String(),
                        Value41 = c.String(),
                        Value42 = c.String(),
                        Value43 = c.String(),
                        Value44 = c.String(),
                        Value45 = c.String(),
                        Value46 = c.String(),
                        Value47 = c.String(),
                        Value48 = c.String(),
                        Value49 = c.String(),
                        Value50 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MT_Objects", t => t.MT_ObjectId, cascadeDelete: true)
                .Index(t => t.MT_ObjectId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Objects", "MT_OrganizationId", "dbo.MT_Organizations");
            DropIndex("dbo.MT_Data", new[] { "MT_ObjectId" });
            DropIndex("dbo.MT_Objects", new[] { "MT_OrganizationId" });
            DropIndex("dbo.MT_Fields", "FieldColumnOffsetIndex");
            DropTable("dbo.MT_Data");
            DropTable("dbo.MT_Organizations");
            DropTable("dbo.MT_Objects");
            DropTable("dbo.MT_Fields");
        }
    }
}
