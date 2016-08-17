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
using Fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using System.Text.RegularExpressions;
using Data.Infrastructure.StructureMap;
using Data.Repositories.Security;
using Data.Repositories.Security.StorageImpl.Cache;
using Fr8.Infrastructure.Data.States;

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

                AddAdmins(uow);
                AddRoles(uow);
                AddTestAccounts(uow);
                AddDefaultProfiles(uow);

                uow.SaveChanges();

                AddPredefinedActivityCategories(uow);
                RenameActivity(uow);
                RegisterTerminals(uow, migrationContainer);
            }
        }

        private void RegisterTerminals(UnitOfWork uow, IContainer container)
        {
            // IMPORTANT: After the migrations have worked out and after you've made sure that 
            // your terminal works fine on the target environment, you need to go to the Terminals page 
            // and manually Approve your terminal to make it available for all users.
            // If you're not an Administrator, ask someone who is to approve your terminal. 

            // Note that if you're adding a Fr8 own terminal, the URL must be port-based. 
            // If you're adding a 3rd party terminal, one created by an external developer
            // or just one which is not deployed by Fr8, it may have any URL but 
            // you need to set the Fr8OwnTerminal argument to true. For details see FR-4945.
            var securityObjectStorage = new SecurityObjectsStorage(uow, container.GetInstance<ISecurityObjectsCache>(), container.GetInstance<ISecurityObjectsStorageProvider>());

            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:10109", "https://terminalInstagram.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:56785", "https://terminalAsana.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:46281", "https://terminalAzure.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:61121", "https://terminalBasecamp2.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:54642", "https://terminalBox.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:39504", "https://terminalSlack.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:53234", "https://terminalDocuSign.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:30700", "https://terminalNotifier.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:51234", "https://terminalSalesforce.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:50705", "https://terminalFr8Core.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:10601", "https://terminalSendGrid.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:30699", "https://terminalTwilio.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:25923", "https://terminalGoogle.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:47011", "https://terminalExcel.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:19760", "https://terminalDropbox.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:30701", "https://terminalPapertrail.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:39768", "https://terminalAtlassian.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:48317", "https://terminalQuickBooks.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:39555", "https://terminalYammer.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:50479", "https://terminalTutorial.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:48675", "https://terminalStatX.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:22666", "https://terminalFacebook.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "http://localhost:59022", "https://terminalTelegram.fr8.co");
            RegisterFr8OwnTerminal(uow, securityObjectStorage, "https://terminalTwitter.fr8.co", "https://terminalTwitter.fr8.co", false);
        }

        private string ExtractPort(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Regex r = new Regex(@"[^/]+?:(?<port>\d+)",
                                     RegexOptions.None, TimeSpan.FromMilliseconds(150));
            Match m = r.Match(url);
            if (m.Success)
                return r.Match(url).Result("${port}");
            else
                return null;
        }

        // ReSharper disable once UnusedMember.Local
        private void RegisterFr8OwnTerminal(UnitOfWork uow, SecurityObjectsStorage securityObjectStorage,string devUrl, string prodUrl = null, bool isFr8OwnTerminal = true)
        {
            var terminalRegistration = new TerminalDO();
            string terminalPort = ExtractPort(devUrl);
            devUrl = NormalizeUrl(devUrl);

            var existingTerminal = uow.TerminalRepository.GetAll().FirstOrDefault(
                x => x.DevUrl != null &&
                    (string.Equals(NormalizeUrl(x.DevUrl), devUrl, StringComparison.OrdinalIgnoreCase)
                    ||
                    (ExtractPort(x.DevUrl) != null && ExtractPort(devUrl) != null &&
                        string.Equals(ExtractPort(x.DevUrl), terminalPort, StringComparison.OrdinalIgnoreCase)
            )));

            if (existingTerminal != null)
            {
                //in order to avoid problems, check if permissions for terminal is already applied. If not, create those permissions 
                securityObjectStorage.SetDefaultRecordBasedSecurityForObject(string.Empty, Roles.StandardUser, existingTerminal.Id, nameof(TerminalDO),Guid.Empty,null, new List<PermissionType>() { PermissionType.UseTerminal });
                securityObjectStorage.SetDefaultRecordBasedSecurityForObject(string.Empty, Roles.Guest, existingTerminal.Id, nameof(TerminalDO), Guid.Empty, null, new List<PermissionType>() { PermissionType.UseTerminal });

                return;
            }

            terminalRegistration.Id = Guid.NewGuid();
            terminalRegistration.Endpoint = devUrl;
            terminalRegistration.DevUrl = devUrl;
            terminalRegistration.ProdUrl = prodUrl;
            terminalRegistration.IsFr8OwnTerminal = isFr8OwnTerminal;
            terminalRegistration.TerminalStatus = TerminalStatus.Undiscovered;
            terminalRegistration.ParticipationState = ParticipationState.Unapproved;

            uow.TerminalRepository.Add(terminalRegistration);
            
            uow.SaveChanges();

            //make the terminal visible for all users
            securityObjectStorage.SetDefaultRecordBasedSecurityForObject(string.Empty, Roles.StandardUser, terminalRegistration.Id, nameof(TerminalDO), Guid.Empty, null, new List<PermissionType>() { PermissionType.UseTerminal });
            securityObjectStorage.SetDefaultRecordBasedSecurityForObject(string.Empty, Roles.Guest, terminalRegistration.Id, nameof(TerminalDO), Guid.Empty, null, new List<PermissionType>() { PermissionType.UseTerminal });
        }

        private static string NormalizeUrl(string terminalUrl)
        {
            if (string.IsNullOrEmpty(terminalUrl))
            {
                return string.Empty;
            }

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
            if (typeof(TConstantsType).BaseType == typeof(Enum))
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

        public static void AddAdmins(IUnitOfWork uow)
        {
            //you can programmatically add Admin accounts here. Now that Fr8 has a Configuration Wizard that is used to create a system administrator account, this is only really used for testing
            //CreateAdmin("test_foo@mail.com", "foobar",uow);
        }

        ///<summary>
        /// Add test users
        /// </summary>
        /// <param name="unitOfWork"></param>
        private static void AddTestAccounts(IUnitOfWork unitOfWork)
        {
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
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.StandardUser, user.Id);
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
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.StandardUser, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.OwnerOfCurrentObject, user.Id);
            user.TestAccount = true;
        }

        /// <summary>
        /// Craete a user with role 'StandardUser'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        public static Fr8AccountDO CreateFr8Account(string userEmail, string curPassword, IUnitOfWork uow)
        {
            var user = uow.UserRepository.GetOrCreateUser(userEmail);
            uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.StandardUser, user.Id);
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.OwnerOfCurrentObject, user.Id);
            user.TestAccount = true;
            return user;
        }

        private void AddPredefinedActivityCategories(IUnitOfWork uow)
        {
            var predefinedCategories = new List<Tuple<Guid, string, string>>()
            {
                new Tuple<Guid, string, string>(ActivityCategories.MonitorId, ActivityCategories.MonitorName, "/Content/icons/monitor-icon-64x64.png"),
                new Tuple<Guid, string, string>(ActivityCategories.ReceiveId, ActivityCategories.ReceiveName, "/Content/icons/get-icon-64x64.png"),
                new Tuple<Guid, string, string>(ActivityCategories.ProcessId, ActivityCategories.ProcessName, "/Content/icons/process-icon-64x64.png"),
                new Tuple<Guid, string, string>(ActivityCategories.ForwardId, ActivityCategories.ForwardName, "/Content/icons/forward-icon-64x64.png"),
                new Tuple<Guid, string, string>(ActivityCategories.SolutionId, ActivityCategories.SolutionName, "/Content/icons/solution-icon-64x64.png")
            };

            foreach (var category in predefinedCategories)
            {
                AddOrUpdateActivityCategory(uow, category.Item1, category.Item2, category.Item3);
            }
        }

        private void AddOrUpdateActivityCategory(IUnitOfWork uow, Guid id, string name, string iconPath)
        {
            var activityTemplateAssignments = new List<ActivityTemplateDO>();

            var existingActivityCategoryByName = uow.ActivityCategoryRepository
                .GetQuery()
                .FirstOrDefault(x => x.Name == name && x.Id != id);

            if (existingActivityCategoryByName != null)
            {
                var existingAssignments = uow.ActivityCategorySetRepository.GetQuery()
                    .Where(x => x.ActivityCategoryId == existingActivityCategoryByName.Id)
                    .ToList();

                foreach (var assignment in existingAssignments)
                {
                    activityTemplateAssignments.Add(assignment.ActivityTemplate);
                    uow.ActivityCategorySetRepository.Remove(assignment);
                }
                uow.SaveChanges();

                uow.ActivityCategoryRepository.Remove(existingActivityCategoryByName);
                uow.SaveChanges();
            }

            var activityCategory = uow.ActivityCategoryRepository
                .GetQuery()
                .FirstOrDefault(x => x.Id == id);

            if (activityCategory == null)
            {
                activityCategory = new ActivityCategoryDO()
                {
                    Id = id,
                    Name = name,
                    IconPath = iconPath,
                    Type = null
                };

                uow.ActivityCategoryRepository.Add(activityCategory);
            }
            else
            {
                activityCategory.IconPath = iconPath;
            }
             
            foreach (var assignedActivityTemplate in activityTemplateAssignments)
            {
                if (!string.IsNullOrEmpty(assignedActivityTemplate.Terminal.Name))
                {
                    uow.ActivityCategorySetRepository.Add(
                    new ActivityCategorySetDO()
                    {
                        Id = Guid.NewGuid(),
                        ActivityCategoryId = activityCategory.Id,
                        ActivityCategory = activityCategory,
                        ActivityTemplateId = assignedActivityTemplate.Id,
                        ActivityTemplate = assignedActivityTemplate
                    });
                } 
            } 

            uow.SaveChanges();
        }

        private void RegisterTestUser(IUnitOfWork uow, string userName, string password, string roleId)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName, roleId);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleId, userDO.Id);
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
            profile.PermissionSets.Add(AddPermissionSet(nameof(PlanNodeDO), false, false, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for ContainerDO
            profile.PermissionSets.Add(AddPermissionSet(nameof(ContainerDO), false, false, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for Terminals
            profile.PermissionSets.Add(AddPermissionSet(nameof(TerminalDO), false, false, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for Users
            profile.PermissionSets.Add(AddPermissionSet(nameof(Fr8AccountDO), false, true, false, profile.Id, "System Administrator Permission Set", uow));

            //default permissions for PageDefinitions
            profile.PermissionSets.Add(AddPermissionSet(nameof(PageDefinitionDO), false, false, false, profile.Id, "System Administrator Permission Set", uow));

            //add standard user to all users without profile 
            var roles = uow.UserRepository.GetQuery().Where(x => x.ProfileId == null).ToList();
            foreach (var item in roles)
            {
                item.ProfileId = profile.Id;
            }

            var adminRole = uow.AspNetRolesRepository.GetQuery().FirstOrDefault(x => x.Name == Roles.Admin);

            var userRoles = uow.AspNetUserRolesRepository.GetQuery().Where(x => x.RoleId == adminRole.Id).Select(l => l.UserId).ToList();
            var fr8Admins = uow.UserRepository.GetQuery().Where(x => userRoles.Contains(x.Id)).ToList();
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
                CreateDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                HasFullAccess = isFullSet
            };

            var repo = new GenericRepository<_PermissionTypeTemplate>(uow);

            permissionSet.Permissions.Clear();
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.CreateObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.ReadObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.EditObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.DeleteObject));
            permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.RunObject));
            if (isFullSet)
            {
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.ViewAllObjects));
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x => x.Id == (int)PermissionType.EditAllObjects));
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

