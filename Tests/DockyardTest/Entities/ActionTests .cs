using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class ActionTests : BaseTest
    {
        [Test]
        [Category("Action")]
        public void Action_Add_CanCreateAction()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                //SETUP
                //create a customer from fixture data
                var curActionDO = FixtureData.TestAction1();

                //EXECUTE
                uow.ActionRepository.Add(curActionDO);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                var savedActionDO = uow.ActionRepository.GetQuery().FirstOrDefault(u => u.Id == curActionDO.Id);
                Assert.NotNull(savedActionDO);
                Assert.AreEqual(curActionDO.UserLabel, savedActionDO.UserLabel);

                var curActionDO2 = FixtureData.TestAction2();

                //EXECUTE
                uow.ActionRepository.Add(curActionDO2);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                var savedActionDO2 = uow.ActionRepository.GetQuery().FirstOrDefault(u => u.Id == curActionDO2.Id);
                Assert.NotNull(savedActionDO2);
                Assert.AreEqual(curActionDO2.UserLabel, savedActionDO2.UserLabel);
            }
        }

        [Test, Ignore]
        [Category("ActionList")]
        public void ActionList_Add_CanCreateActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
              
                //SETUP
                //create a customer from fixture data
                var curActionListDO = FixtureData.TestEmptyActionList();

                //EXECUTE
                uow.ActionListRepository.Add(curActionListDO);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                var savedActionListDO = uow.ActionListRepository.GetQuery().FirstOrDefault(u => u.Id == curActionListDO.Id);
                Assert.NotNull(savedActionListDO);
                Assert.AreEqual(curActionListDO.Name, savedActionListDO.Name);

                var curActionListDO2 = FixtureData.TestActionList();

                //EXECUTE
                uow.ActionListRepository.Add(curActionListDO2);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                var savedActionListDO2 = uow.ActionListRepository.GetQuery().FirstOrDefault(u => u.Id == curActionListDO2.Id);

                Assert.NotNull(curActionListDO2);
                Assert.NotNull(savedActionListDO2);
                Assert.AreEqual(curActionListDO2.Name, savedActionListDO2.Name);
                Assert.AreEqual(curActionListDO2.Actions.FirstOrDefault().Id,
                    savedActionListDO2.Actions.FirstOrDefault().Id);
            }
        }
    }
}