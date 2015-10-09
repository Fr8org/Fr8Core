using System.Collections.Generic;
using System.Data.Entity;
using Core.Interfaces;
using Core.StructureMap;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using RestSharp.Serializers;

namespace DockyardTest.Infrastructure
{
    class MultiTenantTests : BaseTest
    {

        [Test]
        public void MTO_Test_CRUD()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //adding organization
                var fixtureOrganization = FixtureData_MTOobjects.TestOrganization();
                uow.MTOrganizationRepository.Add(fixtureOrganization);
                uow.SaveChanges();
                fixtureOrganization = uow.MTOrganizationRepository.GetByKey(1);

                //adding object to repo
                var obj = FixtureData___MultiTenantObjectSubClass.TestData1();
                obj.fr8AccountId = fixtureOrganization.Id;
                uow.MultiTenantObjectRepository.Add(uow, obj);
                uow.SaveChanges();


                obj.MT_DataId = 1;
                var obj2 = uow.MultiTenantObjectRepository.GetByKey(uow, obj.MT_DataId);

                XmlSerializer xmlSerializer = new XmlSerializer();

                var str_obj1 = xmlSerializer.Serialize(obj);
                var str_obj2 = xmlSerializer.Serialize(obj2);
                //Add & Get test
                Assert.AreEqual(str_obj1, str_obj2);


                //Edit test
                obj.Status = "newvalue";
                uow.MultiTenantObjectRepository.Update(uow, obj);
                uow.SaveChanges();
                obj2 = uow.MultiTenantObjectRepository.GetByKey(uow, obj.MT_DataId);
                str_obj1 = xmlSerializer.Serialize(obj);
                str_obj2 = xmlSerializer.Serialize(obj2);
                Assert.AreEqual(str_obj1, str_obj2);


                //Delete test
                uow.MultiTenantObjectRepository.Remove(uow, 1);
                obj2 = uow.MultiTenantObjectRepository.GetByKey(uow, 1);
                Assert.AreEqual(obj2, null);
            }
        }
    }
}
