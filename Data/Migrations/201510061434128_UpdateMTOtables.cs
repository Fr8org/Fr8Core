namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMTOtables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MT_FieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false),
                        AssemblyName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.MT_Fields", "MT_FieldType_Id", c => c.Int());
            AddColumn("dbo.MT_Objects", "MT_FieldType_Id", c => c.Int());
            CreateIndex("dbo.MT_Fields", "MT_FieldType_Id");
            CreateIndex("dbo.MT_Objects", "MT_FieldType_Id");
            AddForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType", "Id");
            AddForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType", "Id");
            DropColumn("dbo.MT_Fields", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MT_Fields", "Type", c => c.Int(nullable: false));
            DropForeignKey("dbo.MT_Objects", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Fields", "MT_FieldType_Id", "dbo.MT_FieldType");
            DropIndex("dbo.MT_Objects", new[] { "MT_FieldType_Id" });
            DropIndex("dbo.MT_Fields", new[] { "MT_FieldType_Id" });
            DropColumn("dbo.MT_Objects", "MT_FieldType_Id");
            DropColumn("dbo.MT_Fields", "MT_FieldType_Id");
            DropTable("dbo.MT_FieldType");
        }
    }
}
