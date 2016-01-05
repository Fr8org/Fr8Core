using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;

namespace terminalTwilio.Tests
{
    [TestFixture]
    public class BaseTest
    {
        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

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
    }
}