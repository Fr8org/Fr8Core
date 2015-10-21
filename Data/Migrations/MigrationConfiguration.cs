using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Data.Repositories;
using Data.States;
using Data.States.Templates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using StructureMap;
using MT_Field = Data.Entities.MT_Field;
//using MT_FieldService = Data.Infrastructure.MultiTenant.MT_Field;

namespace Data.Migrations
{
    public sealed partial class MigrationConfiguration : DbMigrationsConfiguration<DockyardDbContext>
    {
        public MigrationConfiguration()
        {
            //Do not ever turn this on! It will break database upgrades
            AutomaticMigrationsEnabled = false;

            //Do not modify this, otherwise migrations will run twice!
            ContextKey = "Data.Infrastructure.DockyardDbContext";
        }

        protected override void Seed(DockyardDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Be sure to use AddOrUpdate when creating seed data - otherwise we will get duplicates! */

            //This is not a mistake that we're using new UnitOfWork, rather than calling the ObjectFactory. 
            //The object factory decides what context to use, based on the environment.
            //In this situation, we need to be sure to use the provided context.

            //This class is _not_ mockable - it's a core part of EF. Some seeding, however, is mockable (see the static function Seed and how MockedKwasantDbContext uses it).


            // Uncomment four following lines to debug Seed method (in case running from NuGet Package Manager Console).
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            // If not running inside web application (i.e. running "Update-Database" in NuGet Package Manager Console),
            // then register IDBContext and IUnitOfWork in StructureMap DI.
            if (HttpContext.Current == null)
            {
                ObjectFactory.Initialize(x => x.AddRegistry<MigrationConsoleSeedRegistry>());
            }

            var uow = new UnitOfWork(context);
            SeedIntoMockDb(uow);

            AddRoles(uow);
            AddAdmins(uow);
            AddDockyardAccounts(uow);
            AddProfiles(uow);

            //AddPlugins(uow);

            AddAuthorizationTokens(uow);
            AddContainerDOForTestingApi(uow);
        }

        //Method to let us seed into memory as well
        public static void SeedIntoMockDb(IUnitOfWork uow)
        {
            SeedConstants(uow);
            SeedInstructions(uow);
        }

        private static void AddRoute(IUnitOfWork uow)
        {
        }

        private static CrateDTO GenerateInitialEventCrate()
        {
            var docusignEventPayload = new EventReportCM
            {
                EventNames = "DocuSign Envelope Sent",
                ExternalAccountId = "docusign_developer@dockyard.company",
            };

            var payloadData = new CrateDTO
            {
                Id = "eec6e3b9-63b5-4f55-af49-b0d40dc8d5e2",
                Label = "Payload Data",
                ManifestId = 0,
                CreateTime = DateTime.Now,
                ManifestType = string.Empty,
                Contents = JsonConvert.SerializeObject(new object[]{
                new
                {
                    Key="EnvelopeId",
                    Value="38b8de65-d4c0-435d-ac1b-87d1b2dc5251"
                },
                new
                {
                    Key="ExternalEventType",
                    Value="38b8de65-d4c0-435d-ac1b-87d1b2dc5251"
                },
                new
                {
                    Key="RecipientId",
                    Value="279a1173-04cc-4902-8039-68b1992639e9"
                }})
            };

            docusignEventPayload.EventPayload.Add(payloadData);

            var eventPayload = new CrateDTO
            {
                Id = "eec6e3b9-63b5-4f55-af49-b0d40dc8d5e2",
                Label = "Standard Event Report",
                ManifestId = 7,
                CreateTime = DateTime.Now,
                ManifestType = "Standard Event Report",
                Contents = JsonConvert.SerializeObject(docusignEventPayload)
            };

            return eventPayload;
        }

        private static void AddContainerDOForTestingApi(IUnitOfWork uow)
        {
            new RouteBuilder("TestTemplate{0B6944E1-3CC5-45BA-AF78-728FFBE57358}").AddCrate(GenerateInitialEventCrate()).Store(uow);
            new RouteBuilder("TestTemplate{77D78B4E-111F-4F62-8AC6-6B77459042CB}")
                .AddCrate(GenerateInitialEventCrate())
                .AddCrate(new CrateDTO
                {
                    Id = "671a5b28-1e80-4fbd-ac62-2f308893192f",
                    Label = "DocuSign Envelope Payload Data",
                    ManifestId = 5,
                    CreateTime = DateTime.Now,
                    ManifestType = "Standard Payload Data",
                    Contents = JsonConvert.SerializeObject(new[]
                    {
                        new {Key = "EnvelopeId", Value="38b8de65-d4c0-435d-ac1b-87d1b2dc5251"}
                    })
                }).Store(uow);

            uow.SaveChanges();
        }

        private static void AddAuthorizationTokens(IUnitOfWork uow)
        {
            AddDocusignAuthToken(uow);
            AddSalesforceAuthToken(uow);
        }

        private static void AddDocusignAuthToken(IUnitOfWork uow)
        {
            // Check that plugin does not exist yet.
            var docusignAuthToken = uow.AuthorizationTokenRepository.GetQuery()
                .Any(x => x.ExternalAccountId == "docusign_developer@dockyard.company");

            // Add new plugin and subscription to repository, if plugin doesn't exist.
            if (!docusignAuthToken)
            {
                var token = new AuthorizationTokenDO();
                token.ExternalAccountId = "docusign_developer@dockyard.company";
                token.Token = "";
                token.UserDO = uow.UserRepository.GetOrCreateUser("alex@edelstein.org");
                var docuSignPlugin = uow.PluginRepository.FindOne(p => p.Name == "pluginDocuSign");
                token.Plugin = docuSignPlugin;
                token.PluginID = docuSignPlugin.Id;
                token.ExpiresAt = DateTime.Now.AddDays(10);

                uow.AuthorizationTokenRepository.Add(token);
                uow.SaveChanges();

            }
        }

        private static void AddSalesforceAuthToken(IUnitOfWork uow)
        {
            var salesforceAuthToken = uow.AuthorizationTokenRepository.GetQuery()
             .Any(x => x.ExternalAccountId == "00561000000JECsAAO");

            // Add new plugin and subscription to repository, if plugin doesn't exist.
            if (!salesforceAuthToken)
            {
                var token = new AuthorizationTokenDO();
                token.ExternalAccountId = "00561000000JECsAAO";
                token.Token = "";
                token.UserDO = uow.UserRepository.GetOrCreateUser("alex@edelstein.org");
                var salesforcePlugin = uow.PluginRepository.FindOne(p => p.Name == "pluginSalesforce");
                token.Plugin = salesforcePlugin;
                token.PluginID = salesforcePlugin.Id;
                token.ExpiresAt = DateTime.Now.AddDays(10);

                uow.AuthorizationTokenRepository.Add(token);
                uow.SaveChanges();

            }
        }

        //This method will automatically seed any constants file
        //It looks for rows which implement IConstantRow<>
        //For example, BookingRequestStateRow implements IConstantRow<BookingRequestState>
        //The below method will then generate a new row for each constant found in BookingRequestState.
        private static void SeedConstants(IUnitOfWork context)
        {
            var constantsToSeed =
                typeof(MigrationConfiguration).Assembly.GetTypes()
                    .Select(t => new
                    {
                        RowType = t,
                        ConstantsType =
                            t.GetInterfaces()
                                .Where(i => i.IsGenericType)
                                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IStateTemplate<>))
                    })
                    .Where(t => t.ConstantsType != null).ToList();


            var seedMethod =
                typeof(MigrationConfiguration).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "SeedConstants" && m.IsGenericMethod);
            if (seedMethod == null)
                throw new Exception("Unable to find SeedConstants method.");

            foreach (var constantToSeed in constantsToSeed)
            {
                var rowType = constantToSeed.RowType;
                var constantType = constantToSeed.ConstantsType.GenericTypeArguments.First();

                var idParam = Expression.Parameter(typeof(int));
                var nameParam = Expression.Parameter(typeof(String));

                //The below uses the .NET Expression builder to construct this:
                // (id, name) => new [rowType] { Id = id, Name = name };
                //We need to build it with the expression builder, as we don't know what type to construct yet, and the method requires type arguments.

                //We can't build constructor intialization with Expressions, so this is the logic for it:
                // 1, we create a variable called 'constructedRowType'. This variable is used within the expressions
                // Note that there are _two_ variables. The first one being a usual C# variable, pointing to an expression.
                // The second one is what's actually used within the block. Examining the expression block will show it as '$constructedRowType'

                //The code generated looks like this:
                // rowType generatedFunction(int id, string name) [for example, BookingRequestStateRow generatedFunction(int id, string name)
                // {
                //     rowType $constructedRowType; [for example, BookingRequestStateRow $constructedRowType]
                //     $constructedRowType = new rowType() [for example, $constructedRowType = new BookingRequestStateRow()]
                //     $constructedRowType.Id = id;
                //     $constructedRowType.Name = name;
                //} //Note that there is no return call. Whatever is last on the expression list will be kept at the top of the stack, acting like a 'return' 
                // (if you've worked with bytecode, it's the same idea).
                //We have 'constructedRowType' as the last expression, which tells us it will be returned

                var constructedRowType = Expression.Variable(rowType, "constructedRowType");
                var fullMethod = Expression.Block(
                new[] { constructedRowType },
                Expression.Assign(constructedRowType, Expression.New(rowType)),
                Expression.Assign(Expression.Property(constructedRowType, "Id"), idParam),
                Expression.Assign(Expression.Property(constructedRowType, "Name"), nameParam),
                constructedRowType);

                //Now we take that expression and compile it. It's still typed as a 'Delegate', but it is now castable to Func<int, string, TConstantDO>
                //For example, it could be Func<int, string, BookingRequestStateRow>

                var compiledExpression = Expression.Lambda(fullMethod, new[] { idParam, nameParam }).Compile();

                //Now we have our expression, we need to call something similar to this:
                //SeedConstants<constantType, rowType>(context, compiledExpression)

                var genericSeedMethod = seedMethod.MakeGenericMethod(constantType, rowType);

                genericSeedMethod.Invoke(null, new object[] { context, compiledExpression });
            }
        }

        //Do not remove. Resharper says it's not in use, but it's being used via reflection
        // ReSharper disable UnusedMember.Local
        private static void SeedConstants<TConstantsType, TConstantDO>(IUnitOfWork uow, Func<int, string, TConstantDO> creatorFunc)
            // ReSharper restore UnusedMember.Local
            where TConstantDO : class, IStateTemplate<TConstantsType>, new()
        {
            FieldInfo[] constants = typeof(TConstantsType).GetFields();
            var instructionsToAdd = (from constant in constants
                                     let name = constant.Name
                                     let value = constant.GetValue(null)
                                     select creatorFunc((int)value, name)).ToList();

            //First, we find rows in the DB that don't exist in our seeding. We delete those.
            //Then, we find rows in our seeding that don't exist in the DB. We create those ones (or update the name).

            var repo = new GenericRepository<TConstantDO>(uow);
            var allRows = new GenericRepository<TConstantDO>(uow).GetAll().ToList();
            foreach (var row in allRows)
            {
                if (!instructionsToAdd.Select(i => i.Id).Contains(row.Id))
                {
                    repo.Remove(row);
                }
            }
            foreach (var row in instructionsToAdd)
            {
                var matchingRow = allRows.FirstOrDefault(r => r.Id == row.Id);
                if (matchingRow == null)
                {
                    matchingRow = row;
                    repo.Add(matchingRow);
                }
                matchingRow.Id = row.Id;
                matchingRow.Name = row.Name;
            }
        }

        private static void SeedInstructions(IUnitOfWork unitOfWork)
        {
            Type[] nestedTypes = typeof(InstructionConstants).GetNestedTypes();
            var instructionsToAdd = new List<InstructionDO>();
            foreach (Type nestedType in nestedTypes)
            {
                FieldInfo[] constants = nestedType.GetFields();
                foreach (FieldInfo constant in constants)
                {
                    string name = constant.Name;
                    object value = constant.GetValue(null);
                    instructionsToAdd.Add(new InstructionDO
                    {
                        Id = (int)value,
                        Name = name,
                        Category = nestedType.Name
                    });
                }
            }

            unitOfWork.InstructionRepository.DBSet.AddOrUpdate(
                    i => i.Id,
                    instructionsToAdd.ToArray()
                );
        }

        private static void AddRoles(IUnitOfWork uow)
        {
            Func<string, string, AspNetRolesDO> creatorFunc = (value, name) => new AspNetRolesDO
            {
                Name = name
            };
            FieldInfo[] constants = typeof(Roles).GetFields();
            var rolesToAdd = (from constant in constants
                              let name = constant.Name
                              let value = constant.GetValue(null)
                              select creatorFunc((string)value, name)).ToList();

            var repo = new GenericRepository<AspNetRolesDO>(uow);
            var existingRows = new GenericRepository<AspNetRolesDO>(uow).GetAll().ToList();
            foreach (var row in existingRows) //Delete old rows that are no longer seeded
            {
                if (!rolesToAdd.Select(i => i.Name).Contains(row.Name))
                {
                    repo.Remove(row);
                }
            }
            foreach (var row in rolesToAdd)
            {
                if (!existingRows.Select(r => r.Name).Contains(row.Name))
                    uow.AspNetRolesRepository.Add(row);
            }
        }

        /// <summary>
        /// Add users with 'Admin' role.
        /// </summary>
        /// <param name="unitOfWork">of type ShnexyKwasantDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private static void AddAdmins(IUnitOfWork unitOfWork)
        {
            CreateAdmin("alex@edelstein.org", "foobar", unitOfWork);
            CreateAdmin("d1984v@gmail.com", "dmitry123", unitOfWork);
            CreateAdmin("y.gnusin@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("alexavrutin@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("mvcdeveloper@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("fr8system_monitor@fr8.company", "123qwe", unitOfWork);

            //CreateAdmin("eschebenyuk@gmail.com", "kate235", unitOfWork);
            //CreateAdmin("mkostyrkin@gmail.com", "mk@1234", unitOfWork);
        }

        /// <summary>
        /// Add users with 'Admin' role.
        /// </summary>
        /// <param name="unitOfWork">of type ShnexyKwasantDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private static void AddDockyardAccounts(IUnitOfWork unitOfWork)
        {
            CreateDockyardAccount("alexlucre1@gmail.com", "lucrelucre", unitOfWork);
            CreateDockyardAccount("diagnostics_monitor@dockyard.company", "testpassword", unitOfWork);
            CreateDockyardAccount("fileupload@dockyard.company", "test123", unitOfWork);
            CreateDockyardAccount("sacre", "printf", unitOfWork);
        }

        /// <summary>
        /// Craete a user with role 'Admin'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        private static Fr8AccountDO CreateAdmin(string userEmail, string curPassword, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Booker, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, user.Id);

            user.TestAccount = false;

            return user;
        }

        /// <summary>
        /// Craete a user with role 'Customer'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        private static Fr8AccountDO CreateDockyardAccount(string userEmail, string curPassword, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, user.Id);

            user.TestAccount = true;

            return user;
        }

        private void AddProfiles(IUnitOfWork uow)
        {
            var users = uow.UserRepository.GetAll().ToList();
            foreach (var user in users)
                uow.UserRepository.AddDefaultProfile(user);
        }

        private void AddSubscription(IUnitOfWork uow, Fr8AccountDO curAccount, PluginDO curPlugin, int curAccessLevel)
        {
            var curSub = new SubscriptionDO()
            {
                Plugin = curPlugin,
                DockyardAccount = curAccount,
                AccessLevel = curAccessLevel
            };

            uow.SubscriptionRepository.Add(curSub);
        }

        private void AddPlugins(IUnitOfWork uow)
        {
            // Create test DockYard account for plugin subscription.
            // var account = CreateDockyardAccount("diagnostics_monitor@dockyard.company", "testpassword", uow);

            AddPlugins(uow, "pluginDocuSign", "localhost:53234", "1", true);
            AddPlugins(uow, "pluginExcel", "localhost:47011", "1", false);
            AddPlugins(uow, "pluginSalesforce", "localhost:51234", "1", false);
            uow.SaveChanges();
        }

        private static void AddPlugins(IUnitOfWork uow, string pluginName, string endPoint,
            string version, bool requiresAuthentication)
        {
            // Check that plugin does not exist yet.
            var pluginExists = uow.PluginRepository.GetQuery().Any(x => x.Name == pluginName);

            // Add new plugin and subscription to repository, if plugin doesn't exist.
            if (!pluginExists)
            {
                // Create plugin instance.
                var pluginDO = new PluginDO()
                {
                    Name = pluginName,
                    PluginStatus = PluginStatus.Active,
                    Endpoint = endPoint,
                    Version = version,
                    RequiresAuthentication = requiresAuthentication
                };

                uow.PluginRepository.Add(pluginDO);
            }
        }


        private void AddActionTemplates(IUnitOfWork uow)
        {
            AddActionTemplate(uow, "Filter Using Run-Time Data", "localhost:46281", "1");
            AddActionTemplate(uow, "Wait For DocuSign Event", "localhost:53234", "1");
            AddActionTemplate(uow, "Extract From DocuSign Envelope", "localhost:53234", "1");
            AddActionTemplate(uow, "Extract Table Data", "localhost:47011", "1");
            uow.SaveChanges();
        }

        private void AddActionTemplate(IUnitOfWork uow, string name, string endPoint, string version)
        {
            var existingActivityTemplateDO = uow.ActivityTemplateRepository
                .GetQuery().Include("Plugin")
                .SingleOrDefault(x => x.Name == name);

            if (existingActivityTemplateDO != null)
                return;

            var curActivityTemplateDO = new ActivityTemplateDO(
                name, version, endPoint, endPoint);
            uow.ActivityTemplateRepository.Add(curActivityTemplateDO);
        }


        //Getting random working time within next 3 days
        private static DateTimeOffset GetRandomEventStartTime()
        {
            TimeSpan timeSpan = DateTime.Now.AddDays(3) - DateTime.Now;
            var randomTest = new Random();
            TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = DateTime.Now;
            while (newDate.TimeOfDay.Hours < 9)
            {
                newDate = newDate.Add(new TimeSpan(1, 0, 0));
            }
            while (newDate.TimeOfDay.Hours > 16)
            {
                newDate = newDate.Add(new TimeSpan(-1, 0, 0));
            }
            return newDate;
        }

    }
}

