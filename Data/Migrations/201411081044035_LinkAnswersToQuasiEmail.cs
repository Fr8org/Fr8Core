using Data.Interfaces;
using StructureMap;

namespace Data.Migrations
{    
    using System.Data.Entity.Migrations;
    
    public partial class LinkAnswersToQuasiEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Answers", "EmailID", c => c.Int(nullable: false));
            CreateIndex("dbo.Answers", "EmailID");
            AddForeignKey("dbo.Answers", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "EmailID", "dbo.Emails");
            DropIndex("dbo.Answers", new[] { "EmailID" });
            DropColumn("dbo.Answers", "EmailID");
        }
    }
}
