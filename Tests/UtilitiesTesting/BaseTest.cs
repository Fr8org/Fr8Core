using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Entities;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using Hub.StructureMap;
using Web.ViewModels;
using Fr8Data.Managers;

namespace UtilitiesTesting
{
    [TestFixture]
    public class BaseTest
    {
        protected ICrateManager CrateManager; 

        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            ConfigureAutoMapper();
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            CrateManager = ObjectFactory.Container.GetInstance<ICrateManager>();

            
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>()) //Get the seeding done first
                uow.SaveChanges();
                
        }



        /// <summary>
        /// Creates an API controller with optional authorization context
        /// </summary>
        /// <typeparam name="TController">API controller type</typeparam>
        /// <param name="userId">User ID. Null or empty if no authorization context needed.</param>
        /// <param name="userRoles">User roles</param>
        /// <param name="claimValues">Claim values to create proper ClaimsIdentity for Identity Framework.</param>
        /// <returns></returns>
        public static TController CreateController<TController>(
                string userId = null,
                string[] userRoles = null,
                Tuple<string, string>[] claimValues = null
            ) where TController : ApiController, new()
        {
            var controller = new TController();

            if (!string.IsNullOrEmpty(userId))
            {
                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

                if (claimValues != null)
                {
                    foreach (var claimValue in claimValues)
                    {
                        claims.Add(new Claim(claimValue.Item1, claimValue.Item2));
                    }
                }

                var identity = new ClaimsIdentity(claims);
                controller.User = new GenericPrincipal(identity, userRoles);
            }

            return controller;
        }

        public static void ConfigureAutoMapper()
        {

            Mapper.CreateMap<Fr8AccountDO, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(account => !string.IsNullOrEmpty(account.PasswordHash)))
                .ForMember(mu => mu.HasDocusignToken, opts => opts.Ignore())
                .ForMember(mu => mu.HasGoogleToken, opts => opts.Ignore())
                .ForMember(mu => mu.GoogleSpreadsheets, opts => opts.Ignore());

            Mapper.CreateMap<ActivityNameDTO, ActivityTemplateDO>()
                  .ForMember(activityTemplateDO => activityTemplateDO.Name, opts => opts.ResolveUsing(e => e.Name))
                  .ForMember(activityTemplateDO => activityTemplateDO.Version, opts => opts.ResolveUsing(e => e.Version));

            Mapper.CreateMap<PlanEmptyDTO, PlanDO>();
            Mapper.CreateMap<PlanDO, PlanEmptyDTO>();
            Mapper.CreateMap<UserVM, EmailAddressDO>()
                .ForMember(userDO => userDO.Address, opts => opts.ResolveUsing(e => e.EmailAddress));
            Mapper.CreateMap<UserVM, Fr8AccountDO>()
                .ForMember(userDO => userDO.Id, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(userDO => userDO.FirstName, opts => opts.ResolveUsing(e => e.FirstName))
                .ForMember(userDO => userDO.LastName, opts => opts.ResolveUsing(e => e.LastName))
                .ForMember(userDO => userDO.UserName, opts => opts.ResolveUsing(e => e.UserName))
                .ForMember(userDO => userDO.EmailAddress, opts => opts.ResolveUsing(e => new EmailAddressDO { Address = e.EmailAddress }))
                .ForMember(userDO => userDO.Roles, opts => opts.Ignore());

            Mapper.CreateMap<ActivityDO, ActivityDTO>();

            Mapper.CreateMap<Fr8AccountDO, UserDTO>()
                .ForMember(dto => dto.EmailAddress, opts => opts.ResolveUsing(e => e.EmailAddress.Address))
                .ForMember(dto => dto.Status, opts => opts.ResolveUsing(e => e.State.Value));
        }
    }
}