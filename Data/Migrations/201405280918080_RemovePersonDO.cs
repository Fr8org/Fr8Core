namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemovePersonDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.People", "EmailAddress_Id", "dbo.EmailAddresses");
            DropForeignKey("dbo.Users", "PersonID", "dbo.People");
            DropForeignKey("dbo.Calendars", "PersonId", "dbo.People");
            DropIndex("dbo.People", new[] { "EmailAddress_Id" });
            DropIndex("dbo.Calendars", new[] { "PersonId" });
            DropIndex("dbo.Users", new[] { "PersonID" });
            AddColumn("dbo.Users", "FirstName", c => c.String());
            AddColumn("dbo.Users", "LastName", c => c.String());
            AddColumn("dbo.Users", "EmailAddressID", c => c.Int(nullable: false));

            Sql(@"update users
set EmailAddressID = people.EmailAddress_ID,
Firstname = people.FirstName,
LastName = people.LastName
FROM users
inner join people on users.personid = people.id");

            AddColumn("dbo.Calendars", "OwnerID", c => c.String(maxLength: 128));
            CreateIndex("dbo.Calendars", "OwnerID");
            CreateIndex("dbo.Users", "EmailAddressID", unique: true, name: "IX_User_EmailAddress");
            AddForeignKey("dbo.Users", "EmailAddressID", "dbo.EmailAddresses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Calendars", "OwnerID", "dbo.Users", "Id");
            DropColumn("dbo.Users", "PersonID");
            DropTable("dbo.People");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(maxLength: 30),
                        LastName = c.String(maxLength: 30),
                        EmailAddress_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "PersonID", c => c.Int(nullable: false));
            DropForeignKey("dbo.Calendars", "OwnerID", "dbo.Users");
            DropForeignKey("dbo.Users", "EmailAddressID", "dbo.EmailAddresses");
            DropIndex("dbo.Users", "IX_User_EmailAddress");
            DropIndex("dbo.Calendars", new[] { "OwnerID" });
            DropColumn("dbo.Calendars", "OwnerID");
            DropColumn("dbo.Users", "EmailAddressID");
            DropColumn("dbo.Users", "LastName");
            DropColumn("dbo.Users", "FirstName");
            CreateIndex("dbo.Users", "PersonID");
            CreateIndex("dbo.Calendars", "PersonId");
            CreateIndex("dbo.People", "EmailAddress_Id");
            AddForeignKey("dbo.Calendars", "PersonId", "dbo.People", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Users", "PersonID", "dbo.People", "Id", cascadeDelete: true);
            AddForeignKey("dbo.People", "EmailAddress_Id", "dbo.EmailAddresses", "Id", cascadeDelete: true);
        }
    }
}
