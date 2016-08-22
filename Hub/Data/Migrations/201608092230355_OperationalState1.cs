namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OperationalState1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._OperationalStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            Sql("INSERT INTO dbo._OperationalStateTemplate(Id, Name) VALUES (0, 'Undiscovered')");
            AddColumn("dbo.Terminals", "OperationalState", c => c.Int(nullable: false, defaultValue: 0));
            CreateIndex("dbo.Terminals", "OperationalState");
            AddForeignKey("dbo.Terminals", "OperationalState", "dbo._OperationalStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Terminals", "OperationalState", "dbo._OperationalStateTemplate");
            DropIndex("dbo.Terminals", new[] { "OperationalState" });
            DropColumn("dbo.Terminals", "OperationalState");
            DropTable("dbo._OperationalStateTemplate");
        }
    }
}
