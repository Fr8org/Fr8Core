namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MT_Tables_Update : DbMigration
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
            
            CreateIndex("dbo.MT_Fields", "Id");
            CreateIndex("dbo.MT_Objects", "Id");
            AddForeignKey("dbo.MT_Fields", "Id", "dbo.MT_FieldType", "Id");
            AddForeignKey("dbo.MT_Objects", "Id", "dbo.MT_FieldType", "Id");
            DropColumn("dbo.MT_Fields", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MT_Fields", "Type", c => c.Int(nullable: false));
            DropForeignKey("dbo.MT_Objects", "Id", "dbo.MT_FieldType");
            DropForeignKey("dbo.MT_Fields", "Id", "dbo.MT_FieldType");
            DropIndex("dbo.MT_Objects", new[] { "Id" });
            DropIndex("dbo.MT_Fields", new[] { "Id" });
            DropTable("dbo.MT_FieldType");
        }
    }
}
