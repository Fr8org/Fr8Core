using System;
﻿﻿using Newtonsoft.Json;
﻿using NUnit.Framework;
﻿using StructureMap;
﻿using Data.Entities;
﻿using Data.Interfaces;
﻿using Hub.Managers.APIManagers.Transmitters.Terminal;
﻿using Hub.Services;
using UtilitiesTesting;
﻿using UtilitiesTesting.Fixtures;
﻿
using File = System.IO.File;


namespace DockyardTest.Integration
{
    [TestFixture]
    public class RunTimeTests : BaseTest
    {

        [Test, Ignore("In Process service it is now expecting CurrentActivity to process.")]
        [Category("IntegrationTests")]
        public void ITest_CanProcessHealthDemo()
        {
            //string email;
            //string id;
            // SETUP
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                ObjectFactory.Configure(x =>
                {
                    x.For<ITerminalTransmitter>().Use<TerminalTransmitter>();
                });

                //create a registered account
                Fr8Account _account = new Fr8Account();              
                var registeredAccount = _account.Register(uow, "devtester", "dev", "tester", "password", "User");
                uow.UserRepository.Add(registeredAccount);
                uow.SaveChanges();

                //create a process template linked to that account
             //   var healthRoute = CreatePlan_healthdemo(uow, registeredAccount);
                uow.SaveChanges();

                string healthPayloadPath = "DockyardTest\\Content\\DocusignXmlPayload_healthdemo.xml";

                var xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory, healthPayloadPath);
                //DocuSignNotification _docuSignNotification = ObjectFactory.GetInstance<DocuSignNotification>();
                //_docuSignNotification.Process(registeredAccount.Id, File.ReadAllText(xmlPayloadFullPath));
            }

            // EXECUTE


            // VERIFY
            //Assert.AreEqual(id, userId);
            //Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors));

        }

        //commented out because it was breaking the build.

        //public PlanDO CreatePlan_healthdemo(IUnitOfWork uow, DockyardAccountDO registeredAccount)
        //{
        //    var jsonSerializer = new global::Utilities.Serializers.Json.JsonSerializer();

        //    var healthPlan = FixtureData.TestRouteHealthDemo();
        //    healthPlan.DockyardAccount = registeredAccount;
        //    uow.PlanRepository.Add(healthPlan);
        //    uow.SaveChanges();
        //   healthSubPlansDO.StartingSubPlan = true;
        //    healthPlan.SubPlans.Add(healthSubrouteDO);

        //    //add processnode to process
        //    var healthSubPlanDO = FixtureData.TestSubrouteHealthDemo();
        //    healthSubPlanDO.ParentTemplateId = healthRoute.Id;
        //    uow.SubrouteRepository.Add(healthSubrouteDO);

        //    //specify that this process node is the starting process node of the template
        //    healthPlan.StartingSubPlanId = healthSubPlanDO.Id;

        //    //add criteria to processnode
        //    var healthCriteria = FixtureData.TestCriteriaHealthDemo();
        //    healthCriteria.SubPlanId = healthSubPlanDO.Id;
        //    uow.CriteriaRepository.Add(healthCriteria);

        //    //add actionlist to processnode
        //    var healthActionList = FixtureData.TestActionListHealth1();
        //    healthActionList.SubPlanID = healthSubPlanDO.Id;
        //    uow.ActionListRepository.Add(healthActionList);

        //   // var healthAction = FixtureData.TestActionHealth1();
        //   // uow.ActionRepository.Add(healthAction);

        //    //add write action to actionlist
        //    var healthWriteAction = FixtureData.TestActionWriteSqlServer1();
        //    healthWriteAction.ParentActivityId = healthActionList.Id;
        //    healthActionList.CurrentActivity = healthWriteAction;

        //    //add field mappings to write action
        //    var health_FieldMappings = FixtureData.TestFieldMappingSettingsDTO_Health();
        //   //REPLACE healthWriteAction.FieldMappingSettings = jsonSerializer.Serialize(health_FieldMappings);

        //    //add configuration settings to write action
        //    var configuration_settings = FixtureData.TestConfigurationSettings_healthdemo();
        //    healthWriteAction.CrateStorage = JsonConvert.SerializeObject(configuration_settings);
        //    uow.ActionRepository.Add(healthWriteAction);

        //    return healthPlan;
        //}
    }
}


//ServerConnection serverConnection = new ServerConnection(connection);
//Server server = new Server(serverConnection);

//// after this line, the default database will be switched to Master
//Database database = server.Databases["demodb_health"];

//// you can still use this database object and server connection to 
//// do certain things against this database, like adding database roles 
//// and users      



//// if you want to execute a script against this database, you have to open 
//// another connection and re-initiliaze the server object
//server.ConnectionContext.Disconnect();

//                connection = new SqlConnection(connectionString);
//serverConnection = new ServerConnection(connection);
//server = new Server(serverConnection);
//server.ConnectionContext.ExecuteNonQuery("CREATE TABLE New (NewId int)");
