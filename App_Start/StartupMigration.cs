using Data.Interfaces;
using Data.States;
using StructureMap;
using Fr8.Infrastructure.Utilities;
using Newtonsoft.Json;
using System.Linq;

namespace HubWeb.App_Start
{
    public class StartupMigration
    {
        public static void CreateSystemUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var configRepository  = ObjectFactory.GetInstance<IConfigRepository>();
                string userEmail = configRepository.Get("SystemUserEmail");
                string curPassword = configRepository.Get("SystemUserPassword");

                var user = uow.UserRepository.GetOrCreateUser(userEmail);
                uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, user.Id);
                user.TestAccount = false;
                
                uow.SaveChanges();
            }
        }

        //Prior to FR-3683 Salesforce refresh tokens were stored in nonsecure part of database
        //This method is intended to save them into key vault
        //This method is not a part of Seed method because at that point of time key vault is not yet configured
        //TODO: delete this method after this is deployed to prod
        public static void MoveSalesforceRefreshTokensIntoKeyVault()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalId = uow.TerminalRepository.GetQuery().Where(x => x.Name == "terminalSalesforce").Select(x => x.Id).FirstOrDefault();
                if (terminalId == 0)
                {
                    return;
                }
                var tokens = uow.AuthorizationTokenRepository.GetPublicDataQuery().Where(x => x.TerminalID == terminalId && x.AdditionalAttributes.StartsWith("refresh_token"));
                foreach (var token in tokens)
                {
                    var actualToken = uow.AuthorizationTokenRepository.FindTokenById(token.Id);
                    var refreshTokenFirstIndex = actualToken.AdditionalAttributes.IndexOf('=') + 1;
                    var refreshTokenLastIndex = actualToken.AdditionalAttributes.IndexOf(';');
                    actualToken.Token = JsonConvert.SerializeObject(new { AccessToken = actualToken.Token, RefreshToken = actualToken.AdditionalAttributes.Substring(refreshTokenFirstIndex, refreshTokenLastIndex - refreshTokenFirstIndex) });
                    actualToken.AdditionalAttributes = actualToken.AdditionalAttributes.Substring(refreshTokenLastIndex + 1);
                }
                uow.SaveChanges();
            }
        }
    }
}