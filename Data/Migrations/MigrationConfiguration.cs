using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Data.Interfaces.MultiTenantObjects;
using Data.Repositories;
using Data.States;
using Data.States.Templates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;
using MT_Field = Data.Entities.MT_Field;
using MT_FieldService = Data.Infrastructure.MultiTenant.MT_Field;

namespace Data.Migrations
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<DockyardDbContext>
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
            // if (System.Diagnostics.Debugger.IsAttached == false)
            // {
            //     System.Diagnostics.Debugger.Launch();
            // }

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
            AddPlugins(uow);
            AddActionTemplates(uow);

            SeedMultiTenantTables(uow);
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
        }

        /// <summary>
        /// Craete a user with role 'Admin'
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <param name="uow"></param>
        /// <returns></returns>
        private static DockyardAccountDO CreateAdmin(string userEmail, string curPassword, IUnitOfWork uow)
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
        private static DockyardAccountDO CreateDockyardAccount(string userEmail, string curPassword, IUnitOfWork uow)
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


        private void AddSubscription(IUnitOfWork uow, DockyardAccountDO curAccount, PluginDO curPlugin, int curAccessLevel)
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
            const string azureSqlPluginName = "AzureSqlServerPluginRegistration_v1";

            // Create test Dockaard account for plugin subscription.
            var account = CreateDockyardAccount("diagnostics_monitor@dockyard.company", "testpassword", uow);

            // Check that plugin does not exist yet.
            var azureSqlPluginExists = uow.PluginRepository.GetQuery()
                .Any(x => x.Name == azureSqlPluginName);

            // Add new plugin and subscription to repository, if plugin doesn't exist.
            if (!azureSqlPluginExists)
            {
                // Create plugin instance.
                var azureSqlPlugin = new PluginDO()
                {
                    Name = azureSqlPluginName,
                    PluginStatus = PluginStatus.Active
                };

                uow.PluginRepository.Add(azureSqlPlugin);

                // Create subscription instance.
                AddSubscription(uow,account,azureSqlPlugin,AccessLevel.User);
               
            }
        }

        private void AddActionTemplates(IUnitOfWork uow)
        {
            AddActionTemplate(uow, "Filter Using Run-Time Data", "FilterUsingRunTimeData", "1");
            uow.SaveChanges();
        }

        private void AddActionTemplate(IUnitOfWork uow, string name, string defaultEndPoint, string version)
        {
            var existingActionTemplateDO = uow.ActionTemplateRepository
                .GetQuery()
                .SingleOrDefault(x => x.Name == name && x.Plugin.Name == defaultEndPoint);

            var curActionTemplateDO = new ActionTemplateDO(
                name, defaultEndPoint, version);

            if (existingActionTemplateDO == null)
            {
                uow.ActionTemplateRepository.Add(curActionTemplateDO);
            }
        }

        private void SeedMultiTenantTables(UnitOfWork uow)
        {
            
            AddMultiTenantOrganizations(uow);
            AddMultiTenantObjects(uow);

            //add field for DocuSignEnvelopeStatusReport Object in DocuSign organization
            int docuSignEnvelopeStatusReportObjectId = GetMultiTenantObjectID(uow, "DocuSign",
                "DocuSignEnvelopeStatusReport");
            
            AddMultiTenantFields(uow, docuSignEnvelopeStatusReportObjectId, new DocuSignEnvelopeStatusReportMTO());

            //add field for DocuSignRecipientStatusReportMTO Object in DocuSign organization
            int docuSignRecipientStatusReportObjectId = GetMultiTenantObjectID(uow, "DocuSign",
                "DocuSignRecipientStatusReport");

            AddMultiTenantFields(uow, docuSignRecipientStatusReportObjectId, new DocuSignRecipientStatusReportMTO());
        }

        private void AddMultiTenantOrganizations(UnitOfWork uow)
        {
            uow.MTOrganizationRepository.Add(new MT_Organization { Name = "Dockyard" });
            uow.MTOrganizationRepository.Add(new MT_Organization { Name = "DocuSign" });

            uow.SaveChanges();
        }

        private void AddMultiTenantObjects(UnitOfWork uow)
        {
            //get organizations
            var orgDockyard = uow.MTOrganizationRepository.GetQuery().First(org => org.Name.Equals("Dockyard"));
            var orgDocuSign = uow.MTOrganizationRepository.GetQuery().First(org => org.Name.Equals("DocuSign"));

            //add MT object for Dockyard
            uow.MTObjectRepository.Add(new MT_Object {Name = "DockyardEvent", MT_OrganizationId = orgDockyard.Id});
            uow.MTObjectRepository.Add(new MT_Object {Name = "DockyardIncident", MT_OrganizationId = orgDockyard.Id});

            //add MT object for DocuSign
            uow.MTObjectRepository.Add(new MT_Object {Name = "DocuSignEnvelopeStatusReport", MT_OrganizationId = orgDocuSign.Id});
            uow.MTObjectRepository.Add(new MT_Object {Name = "DocuSignRecipientStatusReport", MT_OrganizationId = orgDocuSign.Id});

            uow.SaveChanges();
        }

        private int GetMultiTenantObjectID(IUnitOfWork uow, string curMtOrganizationName, string curMtObjectName)
        {
            return
                uow.MTObjectRepository.FindOne(
                    obj => obj.MT_Organization.Name.Equals(curMtOrganizationName) && obj.Name.Equals(curMtObjectName))
                    .Id;
        }

        private void AddMultiTenantFields(IUnitOfWork uow, int curObjectId, MultiTenantObject curMto)
        {
            var _mtField = new MT_FieldService();

            var typeMap = new Dictionary<Type, MT_FieldType>()
            {
                {typeof (string), MT_FieldType.String},
                {typeof (int), MT_FieldType.Int},
                {typeof (bool), MT_FieldType.Boolean}
            };

            //get the current MTO fields
            Type curMtoType = curMto.GetType();
            var curMtoProperties = curMtoType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            //for each field
            foreach (PropertyInfo propertyInfo in curMtoProperties)
            {

                MT_Field curMtField = new MT_Field();

                //set property name, type and Object ID
                curMtField.Name = propertyInfo.Name;
                curMtField.Type = typeMap[propertyInfo.PropertyType];
                curMtField.MT_ObjectId = curObjectId;

                curMtField.FieldColumnOffset =
                    _mtField.GetFieldColumnOffset(uow, curMtField.Name, curMtField.MT_ObjectId) ??
                    _mtField.GenerateFieldColumnOffset(uow, curMtField.MT_ObjectId);

                if (curMtField.FieldColumnOffset > 50)
                {
                    throw new InvalidOperationException(
                        "MTO fields are limited to only 50 Columns. Check your MTO to keep its number of Properties to be less than or equal to 50.");
                }

                _mtField.Add(uow, curMtField);
            }

            uow.SaveChanges();
        }
        

        //Getting random working time within next 3 days
        private static DateTimeOffset GetRandomEventStartTime()
        {
            TimeSpan timeSpan = DateTime.Now.AddDays(3) - DateTime.Now;
            var randomTest = new Random();
            TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = DateTime.Now + newSpan;
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

