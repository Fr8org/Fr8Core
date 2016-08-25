using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Utilities;
using Microsoft.AspNet.Identity;

namespace Hub.Interfaces
{
    public interface IFr8Account
    {
        void Create(IUnitOfWork uow, Fr8AccountDO submittedDockyardAccountData);
        Task<Tuple<LoginStatus, string>> CreateAuthenticateGuestUser();
        Task ForgotPasswordAsync(string userEmail);
        IEnumerable<PlanDO> GetActivePlans(string userId);
        IEnumerable<ContainerDO> GetContainerList(string userId);
        string GetDisplayName(Fr8AccountDO curDockyardAccount);
        Fr8AccountDO GetExisting(IUnitOfWork uow, string emailAddress);
        CommunicationMode GetMode(Fr8AccountDO dockyardAccountDO);
        Fr8AccountDO GetSystemUser();
        string GetUserId(string emailAddress);
        string GetUserRole(string userName);
        bool IsCurrentUserInAdminRole();
        bool IsValidHashedPassword(Fr8AccountDO dockyardAccountDO, string password);
        LoginStatus Login(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO, string password, bool isPersistent);
        Task<Tuple<LoginStatus, string>> ProcessLoginRequest(string username, string password, bool isPersistent, HttpRequestMessage request = null);
        RegistrationStatus ProcessRegistrationRequest(IUnitOfWork uow, string email, string password, OrganizationDO organizationDO, bool isNewOrganization, string anonimousId);
        Fr8AccountDO Register(IUnitOfWork uow, string userName, string firstName, string lastName, string password, string roleID, OrganizationDO organizationDO = null);
        Task<IdentityResult> ResetPasswordAsync(string userId, string code, string password);
        void Update(IUnitOfWork uow, Fr8AccountDO submittedDockyardAccountData, Fr8AccountDO existingDockyardAccount);
        Task<RegistrationStatus> UpdateGuestUserRegistration(IUnitOfWork uow, string email, string password, string tempEmail, OrganizationDO organizationDO = null);
        void UpdatePassword(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO, string password);
        bool VerifyMinimumRole(string minAuthLevel, string curUserId, IUnitOfWork uow);
        bool CheckForExistingAdminUsers();
        Task CreateAdminAccount(string userEmail, string curPassword);
    }
}