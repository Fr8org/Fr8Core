using System.Data.Entity.ModelConfiguration.Configuration;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.AspNet.Identity;

namespace Data.Migrations
{
    using Infrastructure;
    using Interfaces;
    using StructureMap;
    using System;
    using System.Data.Entity.Migrations;
    //This migration added test user
    //To complete it, we make add rows to tables _UserStateTemplate, EmailAddress, AspNetUsers and finally User.
    public partial class Add_Test_User : System.Data.Entity.Migrations.DbMigration
    {
        private string email = CloudConfigurationManager.GetSetting("TestUserAccountName");
        private string name = "IntegrationTestRunner";
        //private string id = Guid.NewGuid().ToString();
        private string password = CloudConfigurationManager.GetSetting("TestUserPassword");
        //private readonly string SECURITY_STAMP = Guid.NewGuid().ToString();
        public override void Up()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MigrationConfiguration.CreateFr8Account(email, password, uow);
            }
            
            //var passwordHasher = new PasswordHasher();
            //var hashedPassword = passwordHasher.HashPassword(PASSWORD);
            //Sql("DELETE FROM [dbo].[_UserStateTemplate] WHERE Name = 'Active'");
            //Sql("INSERT INTO [dbo].[_UserStateTemplate] (Id, Name) VALUES (1, 'Active')");
            //Sql(string.Format("DELETE FROM [dbo].[EmailAddresses] WHERE Address = '{0}'", EMAIL));
            //var INSERT_TO_EMAIL_ADDRESSES =
            //    string.Format(@"INSERT INTO [dbo].[EmailAddresses] (Address, LastUpdated, CreateDate)
            //                             VALUES ('{0}',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)", EMAIL);
            //Sql(INSERT_TO_EMAIL_ADDRESSES);
            //Sql(string.Format("DELETE FROM [dbo].[AspNetUsers] WHERE UserName = '{0}'", NAME));
            //var INSERT_TO_ASP_NET_USERS =
            //    string.Format(@"INSERT INTO [dbo].[AspNetUsers] (Id, PasswordHash, SecurityStamp, PhoneNumberConfirmed, EmailConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
            //                             VALUES ('{0}', '{1}', '{2}', 0, 0, 0, 0, 0,'{3}')", ID, hashedPassword, SECURITY_STAMP, NAME);
            //Sql(INSERT_TO_ASP_NET_USERS);
            //Sql(string.Format("DELETE FROM [dbo].[Users] WHERE EmailAddressId = (SELECT Id FROM [dbo].[EmailAddresses] WHERE Address = '{0}')", EMAIL));
            //var INSERT_TO_USERS =
            //    string.Format(@"INSERT INTO [dbo].[Users] (Id, TestAccount, EmailAddressId, State, CreateDate, LastUpdated)
            //                             VALUES ('{0}', 1, (SELECT Id FROM [dbo].[EmailAddresses] WHERE Address = '{1}'), 1, CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)", ID, EMAIL);
            //Sql(INSERT_TO_USERS);
        }

        public override void Down()
        {
            //It is deemed that testing account shound not be deleted and should have been created upon the DB creation
        }
    }
}
