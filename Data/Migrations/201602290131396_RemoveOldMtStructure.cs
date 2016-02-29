namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOldMtStructure : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects");
            DropForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects");
            DropIndex("dbo.MT_Fields", "FieldColumnOffsetIndex");
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Objects", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Data", new[] { "MT_ObjectId" });
            DropTable("dbo.MT_Fields");
            DropTable("dbo.MT_FieldType");
            DropTable("dbo.MT_Objects");
            DropTable("dbo.MT_Data");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MT_Data",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GUID = c.Guid(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        fr8AccountId = c.String(nullable: false),
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
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Objects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ManifestId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        MT_FieldType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_FieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false),
                        AssemblyName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MT_Fields",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        FieldColumnOffset = c.Int(nullable: false),
                        MT_ObjectId = c.Int(nullable: false),
                        MT_FieldType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.MT_Data", "MT_ObjectId");
            CreateIndex("dbo.MT_Objects", "MT_FieldType_Id");
            CreateIndex("dbo.MT_Fields", "MT_FieldType_Id");
            CreateIndex("dbo.MT_Fields", new[] { "MT_ObjectId", "Name", "FieldColumnOffset" }, name: "FieldColumnOffsetIndex");
            AddForeignKey("dbo.MT_Data", "MT_ObjectId", "dbo.MT_Objects", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MT_Fields", "MT_ObjectId", "dbo.MT_Objects", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType", "Id");
            AddForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType", "Id", cascadeDelete: true);
        }
    }
}
