using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_State_to_UserDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._UserStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "State", c => c.Int(nullable: false));

            const string insertStatement = @"
INSERT INTO [dbo].[_UserStateTemplate] (Id, Name)
VALUES ({0}, '{1}')
";
            Sql(String.Format(insertStatement, UserState.Active, "Active"));
            Sql(String.Format(insertStatement, UserState.Deleted, "Deleted"));
            Sql(String.Format(insertStatement, UserState.Suspended, "Suspended"));

            Sql(@"UPDATE dbo.Users SET State = " + UserState.Active);
            CreateIndex("dbo.Users", "State");
            AddForeignKey("dbo.Users", "State", "dbo._UserStateTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "State", "dbo._UserStateTemplate");
            DropIndex("dbo.Users", new[] { "State" });
            DropColumn("dbo.Users", "State");
            DropTable("dbo._UserStateTemplate");
        }
    }
}
