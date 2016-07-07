using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Repositories;
using Data.States;
using Data.States.Templates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;

namespace Data.Migrations
{
    public sealed partial class MigrationConfiguration : DbMigrationsConfiguration<DockyardDbContext>
    {
        public MigrationConfiguration()
        {
            //Do not ever turn this on! It will break database upgrades
            AutomaticMigrationsEnabled = false;

            CommandTimeout = 60 * 15;

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
           /* if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }*/

            using (var migrationContainer = new Container())
            {
                migrationContainer.Configure(x => x.AddRegistry<MigrationConsoleSeedRegistry>());

                var uow = new UnitOfWork(context, migrationContainer);

                UpdateRootPlanNodeId(uow);

                SeedIntoMockDb(uow);

                AddRoles(uow);
                AddAdmins(uow);
                AddDockyardAccounts(uow);
                AddTestAccounts(uow);
                AddDefaultProfiles(uow);

                uow.SaveChanges();

                AddWebServices(uow);
                AddTestUser(uow);
                UpdateTerminalClientVisibility(uow);
                RenameActivity(uow);
                RegisterTerminals(uow);
            }
        }

        private void RegisterTerminals(UnitOfWork uow)
        {
            // Example of terminal registration: RegisterTerminal (uow, "localhost:12345");   
        }
        
        // ReSharper disable once UnusedMember.Local
        private void RegisterTerminal(UnitOfWork uow, string terminalEndpoint)
        {
            var terminalRegistration = new TerminalRegistrationDO();

            terminalEndpoint = ExtractTerminalAuthority(terminalEndpoint);

            if (uow.TerminalRegistrationRepository.GetAll().FirstOrDefault(x => string.Equals(ExtractTerminalAuthority(x.Endpoint), terminalEndpoint, StringComparison.OrdinalIgnoreCase)) != null)
            {
                return;
            }

            terminalRegistration.Endpoint = terminalEndpoint;

            uow.TerminalRegistrationRepository.Add(terminalRegistration);
            uow.SaveChanges();
        }

        private static string ExtractTerminalAuthority(string terminalUrl)
        {
            var terminalAuthority = terminalUrl;

            if (!terminalUrl.Contains("http:") & !terminalUrl.Contains("https:"))
            {
                terminalAuthority = "http://" + terminalUrl.TrimStart('\\', '/');
            }
            
            return terminalAuthority.TrimEnd('\\', '/');
        }
        
        private void RenameActivity(UnitOfWork uow)
        {
            var activities = uow.ActivityTemplateRepository.GetAll();
            var activityToRename = activities.FirstOrDefault(x => x.Name == "TestAndBranch");
            if (activityToRename != null)
            {
                activityToRename.Name = "Make_A_Decision";
                activityToRename.Label = "Make a Decision";
            }
        }

        private void UpdateTerminalClientVisibility(UnitOfWork uow)
        {
            var activities = uow.ActivityTemplateRepository.GetAll();
            foreach (var activity in activities)
            {
                activity.ClientVisibility = activity.Name != "Monitor_DocuSign_Envelope_Activity";
            }
            uow.SaveChanges();
        }

        //Method to let us seed into memory as well
        public static void SeedIntoMockDb(IUnitOfWork uow)
        {
            SeedConstants(uow);
            SeedInstructions(uow);
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
            var constants = typeof(TConstantsType).GetFields();
            List<TConstantDO> instructionsToAdd;
            if (typeof (TConstantsType).BaseType == typeof (Enum))
            {
                var enumValues = Enum.GetValues(typeof(TConstantsType)).Cast<TConstantsType>().ToList();

                instructionsToAdd = (from constant in enumValues
                    let name = constant.ToString()
                    let value = constant
                    select creatorFunc(Convert.ToInt32(value), name)).ToList();
            }
            else
            {
                instructionsToAdd = (from constant in constants
                                         let name = constant.Name
                                         let value = constant.GetValue(null)
                                         select creatorFunc((int)value, name)).ToList();
            }
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
            var nestedTypes = typeof(InstructionConstants).GetNestedTypes();
            var instructionsToAdd = new List<InstructionDO>();
            foreach (var nestedType in nestedTypes)
            {
                var constants = nestedType.GetFields();
                foreach (var constant in constants)
                {
                    var name = constant.Name;
                    var value = constant.GetValue(null);
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
            var constants = typeof(Roles).GetFields();
            var rolesToAdd = (from constant in constants
                              let name = constant.Name
                              let value = constant.GetValue(null)
                              select creatorFunc((string)value, name)).ToList();

            var existingRows = new GenericRepository<AspNetRolesDO>(uow).GetAll().ToList();
            
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
            CreateAdmin("bahadir.bb@gmail.com", "123456ab", unitOfWork);
            CreateAdmin("omer@fr8.co", "123456ab", unitOfWork);
            CreateAdmin("alp@fr8.co", "123qwe", unitOfWork);
            CreateAdmin("emre@fr8.co", "123qwe", unitOfWork);
            CreateAdmin("mvcdeveloper@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("maki.gjuroski@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("fr8system_monitor@fr8.company", "123qwe", unitOfWork);
            CreateAdmin("teh.netaholic@gmail.com", "123qwe", unitOfWork);
            CreateAdmin("farrukh.normuradov@gmail.com", "123qwe", unitOfWork);
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
            CreateDockyardAccount("integration_test_runner@fr8.company", "fr8#s@lt!", unitOfWork);
        }
        /// <summary>
        /// Add test user with 'Admin' role
        /// </summary>
        /// <param name="unitOfWork"></param>
        private static void AddTestAccounts(IUnitOfWork unitOfWork)
        {
            CreateTestAccount("integration_test_runner@fr8.company", "fr8#s@lt!", "IntegrationTestRunner", unitOfWork);
        }

        /// <summary>
        /// Create a user with role 'Admin'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        private static void CreateAdmin(string userEmail, string curPassword, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Booker, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.OwnerOfCurrentObject, user.Id);
            user.TestAccount = false;
        }

        private static void CreateTestAccount(string userEmail, string curPassword, string userName, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            if (user != null)
            {
                return;
            }
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Booker, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, user.Id);
            user.TestAccount = true;
        }

        /// <summary>
        /// Craete a user with role 'Customer'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        public static Fr8AccountDO CreateDockyardAccount(string userEmail, string curPassword, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, user.Id);
            user.TestAccount = true;
            return user;
        }
        
        private void AddWebServices(IUnitOfWork uow)
        {
            var terminalToWs = new Dictionary<string, string>
	        {
	            {"terminalSalesforce", "Salesforce"},
	            {"terminalFr8Core", "fr8 Core"},
	            {"terminalDocuSign", "DocuSign"},
	            {"terminalSlack", "Slack"},
	            {"terminalTwilio", "Twilio"},
	            {"terminalAzure", "Microsoft Azure"},
	            {"terminalExcel", "Excel"},
                {"terminalGoogle", "Google"},
                {"terminalPapertrail", "Papertrail"}
	        };

            var wsToId = new Dictionary<string, int>();

            AddWebService(uow, "AWS", "/Content/icons/web_services/aws-icon-64x64.png");
            AddWebService(uow, "Slack", "/Content/icons/web_services/slack-icon-64x64.png");
            AddWebService(uow, "DocuSign", "/Content/icons/web_services/docusign-icon-64x64.png");
            AddWebService(uow, "Microsoft Azure", "/Content/icons/web_services/ms-azure-icon-64x64.png");
            AddWebService(uow, "Excel", "/Content/icons/web_services/ms-excel-icon-64x64.png");
            AddWebService(uow, "Built-In Services", "/Content/icons/web_services/fr8-core-icon-64x64.png");
            AddWebService(uow, "Salesforce", "/Content/icons/web_services/salesforce-icon-64x64.png");
            AddWebService(uow, "SendGrid", "/Content/icons/web_services/sendgrid-icon-64x64.png");
            AddWebService(uow, "Dropbox", "/Content/icons/web_services/dropbox-icon-64x64.png");
            AddWebService(uow, "Atlassian", "/Content/icons/web_services/jira-icon-64x64.png");
            AddWebService(uow, "UnknownService", "/Content/icons/web_services/unknown-service.png");

            foreach (var webServiceDo in uow.WebServiceRepository.GetAll())
            {
                if (webServiceDo.Name != null)
                {
                    wsToId[webServiceDo.Name] = webServiceDo.Id;
                }
            }

            foreach (var activity in uow.ActivityTemplateRepository.GetQuery().Include(x => x.Terminal))
            {
                string wsName;
                int wsId;

                if (terminalToWs.TryGetValue(activity.Terminal.Name, out wsName) && wsToId.TryGetValue(wsName, out wsId))
                {
                    activity.WebServiceId = wsId;
                }
            }

            uow.SaveChanges();
        }

        private void AddTestUser(IUnitOfWork uow)
        {
            const string email = "integration_test_runner@fr8.company";
            const string password = "fr8#s@lt!";

            //check if we know this email address

            var existingEmailAddressDO = uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == email);
            if (existingEmailAddressDO != null)
            {
                RegisterTestUser(uow, email, password, Roles.Customer);
            }
            else
            {
                RegisterTestUser(uow, email, password, Roles.Customer);
            }

            uow.SaveChanges();
        }

        private void RegisterTestUser(IUnitOfWork uow, string userName, string password, string roleId)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName, roleId);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleId, userDO.Id);
        }

        private void AddWebService(IUnitOfWork uow, string name, string iconPath)
        {
            var isWsExists = uow.WebServiceRepository.GetQuery().Any(x => x.Name == name);

            if (isWsExists)
            {
                return;
            }
            var webServiceDO = new WebServiceDO
                               {
                                   Name = name,
                                   IconPath = iconPath
                               };

            uow.WebServiceRepository.Add(webServiceDO);
        }
        
        private void UpdateRootPlanNodeId(IUnitOfWork uow)
        {
            var anyRootIdFlag = uow.PlanRepository.GetNodesQueryUncached().Any(x => x.RootPlanNodeId != null);

            if (anyRootIdFlag)
            {
                return;
            }

            var fullTree = uow.PlanRepository.GetNodesQueryUncached().ToList();

            var parentChildMap = new Dictionary<Guid, List<PlanNodeDO>>();
            foreach (var planNode in fullTree.Where(x => x.ParentPlanNodeId.HasValue))
            {
                List<PlanNodeDO> planNodes;
                if (!parentChildMap.TryGetValue(planNode.ParentPlanNodeId.Value, out planNodes))
                {
                    planNodes = new List<PlanNodeDO>();
                    parentChildMap.Add(planNode.ParentPlanNodeId.Value, planNodes);
                }

                planNodes.Add(planNode);
            }

            var roots = fullTree.Where(x => parentChildMap.ContainsKey(x.Id) && !x.ParentPlanNodeId.HasValue);

            var queue = new Queue<PlanNodeDO>();
            foreach (var root in roots)
            {
                root.RootPlanNodeId = root.Id;
                queue.Enqueue(root);
            }

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (!parentChildMap.ContainsKey(node.Id))
                {
                    continue;
                }

                var childNodes = parentChildMap[node.Id];
                foreach (var childNode in childNodes)
                {
                    childNode.RootPlanNodeId = node.RootPlanNodeId;
                    queue.Enqueue(childNode);
                }
            }

            uow.SaveChanges();
        }

        public static void AddDefaultProfiles(IUnitOfWork uow)
        {
            //create 'Fr8 Admin' Profile 
            var fr8AdminProfile = uow.ProfileRepository.GetQuery().FirstOrDefault(x => x.Name == DefaultProfiles.Fr8Administrator);
            if (fr8AdminProfile == null)
            {
                fr8AdminProfile = new ProfileDO
                {
                    Id = Guid.NewGuid(),
                    Name = DefaultProfiles.Fr8Administrator,
                    Protected = true,
                    PermissionSets = new List<PermissionSetDO>()
                };
                uow.ProfileRepository.Add(fr8AdminProfile);
            }
            else
            {
                fr8AdminProfile.Protected = true;
            }
            
            //create 'System Administrator' Profile 
            var profile = uow.ProfileRepository.GetQuery().FirstOrDefault(x => x.Name == DefaultProfiles.SystemAdministrator);
            if (profile == null)
            {
                profile = new ProfileDO
                {
                    Id = Guid.NewGuid(),
                    Name = DefaultProfiles.SystemAdministrator,
                    Protected = true,
                    PermissionSets = new List<PermissionSetDO>()
                };
                uow.ProfileRepository.Add(profile);
            }
            else
            {
                profile.Protected = true;
            }

            //create 'Standard User' profile
            var standardProfile = uow.ProfileRepository.GetQuery().FirstOrDefault(x => x.Name == DefaultProfiles.StandardUser);
            if (standardProfile == null)
            {
                standardProfile = new ProfileDO()
                {
                    Id = Guid.NewGuid(),
                    Name = DefaultProfiles.StandardUser,
                    Protected = true
                };
                uow.ProfileRepository.Add(standardProfile);
            }
            else
            {
                standardProfile.Protected = true;
            }

            //presave needed here for permissionSetPermissions table inserts
            uow.SaveChanges();

            fr8AdminProfile.PermissionSets.Clear();
            //default permissions for Plans and PlanNodes
            fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(PlanNodeDO), true, false, false, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));

            //default permissions for ContainerDO
            fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(ContainerDO), true, false, false, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));

            //default permissions for Terminals
            fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(TerminalDO), true, false, false, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));

            //default permissions for Users
            fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(Fr8AccountDO), true, false, true, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));
            
            //default permissions for PageDefinitions
            fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(PageDefinitionDO), true, false, false, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));

            profile.PermissionSets.Clear();
            //default permissions for Plans and PlanNodes
            profile.PermissionSets.Add(AddPermissionSet(nameof(PlanNodeDO), true, false, false, profile.Id,"System Administrator Permission Set", uow));

            //default permissions for ContainerDO
            profile.PermissionSets.Add(AddPermissionSet(nameof(ContainerDO), true, false, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for Terminals
            profile.PermissionSets.Add(AddPermissionSet(nameof(TerminalDO), true, false, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for Users
            profile.PermissionSets.Add(AddPermissionSet(nameof(Fr8AccountDO), true, true, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for PageDefinitions
            profile.PermissionSets.Add(AddPermissionSet(nameof(PageDefinitionDO), true, false, false, profile.Id, "System Administrator Permission Set", uow));

            //add standard user to all users without profile 
            var roles = uow.UserRepository.GetQuery().Where(x => x.ProfileId == null).ToList();
            foreach (var item in roles)
            {
                item.ProfileId = profile.Id;
            }

            var adminRole = uow.AspNetRolesRepository.GetQuery().FirstOrDefault(x => x.Name == Roles.Admin);

            var userRoles = uow.AspNetUserRolesRepository.GetQuery().Where(x => x.RoleId == adminRole.Id).Select(l=>l.UserId).ToList();
            var fr8Admins = uow.UserRepository.GetQuery().Where(x=> userRoles.Contains(x.Id)).ToList();
            foreach (var user in fr8Admins)
            {
                user.ProfileId = fr8AdminProfile.Id;
            }
            
            standardProfile.PermissionSets.Clear();
            //default permissions for Plans and PlanNodes
            standardProfile.PermissionSets.Add(AddPermissionSet(nameof(PlanNodeDO), false, false, false, standardProfile.Id, "Standard User Permission Set", uow));

            //default permissions for ContainerDO
            standardProfile.PermissionSets.Add(AddPermissionSet(nameof(ContainerDO), false, false, false, standardProfile.Id, "Standard User Permission Set", uow));

            //default permissions for Terminals
            standardProfile.PermissionSets.Add(AddPermissionSet(nameof(TerminalDO), false, false, false, standardProfile.Id, "Standard User Permission Set", uow));

            //default permissions for Users
            standardProfile.PermissionSets.Add(AddPermissionSet(nameof(Fr8AccountDO), false, false, false, standardProfile.Id, "Standard User Permission Set", uow));
        }

        private static PermissionSetDO AddPermissionSet(
            string objectType, 
            bool isFullSet, 
            bool hasManageInternalUsers, 
            bool hasManageFr8Users,
            Guid profileId, 
            string name, 
            IUnitOfWork uow)
        {
            var permissionSet = uow.PermissionSetRepository.GetQuery().FirstOrDefault(x => x.Name == name && x.ObjectType == objectType) ?? new PermissionSetDO
                                                                                                                                            {
                                                                                                                                                Name = name,
                                                                                                                                                ProfileId = profileId,
                                                                                                                                                ObjectType = objectType,
                                                                                                                                                CreateDate = DateTimeOffset.Now,
                                                                                                                                                LastUpdated = DateTimeOffset.Now,
                                                                                                                                                HasFullAccess = isFullSet
                                                                                                                                            };

            var repo = new GenericRepository<_PermissionTypeTemplate>(uow);

            permissionSet.Permissions.Clear();
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x=>x.Id == (int) PermissionType.CreateObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.ReadObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.EditObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.DeleteObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.RunObject));
            if (isFullSet)
            {
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.ViewAllObjects));
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int) PermissionType.ModifyAllObjects));
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.EditPageDefinitions));
            }

            if (hasManageFr8Users)
            {
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.ManageFr8Users));
            }

            if (hasManageInternalUsers)
            {
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.ManageInternalUsers));
            }

            if (permissionSet.Id == Guid.Empty)
            {
                permissionSet.Id = Guid.NewGuid();
                uow.PermissionSetRepository.Add(permissionSet);
            }

            return permissionSet;
        }

    }
}

