using System;
using System.Collections.Generic;
using System.Data.Entity;
using RestSharp.Serializers;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.Manifests;

namespace DockyardTest.Infrastructure
{
    class MultiTenantTests : BaseTest
    {

        [Test]
        public void CanUpdateUsingPrimaryKey()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                //adding object to repo
                var manifest = FixtureData___MultiTenantObjectSubClass.TestData1();
                uow.MultiTenantObjectRepository.Add(uow, manifest, userDO.Id);
                uow.SaveChanges();

                //test "Update()"
                manifest.Status = "newstatus";
                uow.MultiTenantObjectRepository.Update(uow, userDO.Id, manifest);
                var manifest_from_MTO = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                XmlSerializer xmlSerializer = new XmlSerializer();
                var str_obj1 = xmlSerializer.Serialize(manifest);
                var str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);
            }
        }


        [Test]
        public void CanAddOrUpdateUsingPrimaryKey()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                //adding object to repo
                var manifest = FixtureData___MultiTenantObjectSubClass.TestData1();
                uow.MultiTenantObjectRepository.Add(uow, manifest, userDO.Id);
                uow.SaveChanges();
                
                
                //test "AddOrUPDATE"
                manifest.Status = "foo";
                uow.MultiTenantObjectRepository.AddOrUpdate(uow, userDO.Id, manifest);
                var manifest_from_MTO = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                var xmlSerializer = new XmlSerializer();
                var str_obj1 = xmlSerializer.Serialize(manifest);
                var str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);
            }
        }

        [Test]
        public void MTO_Test_CRUD()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                //adding object to repo
                var manifest = FixtureData___MultiTenantObjectSubClass.TestData1();
                uow.MultiTenantObjectRepository.Add(uow, manifest, userDO.Id);
                uow.SaveChanges();

                //test "Update()"
                manifest.Status = "newstatus";
                uow.MultiTenantObjectRepository.Update(uow, userDO.Id, manifest, keyProperty: b => b.EnvelopeId);
                var manifest_from_MTO = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                XmlSerializer xmlSerializer = new XmlSerializer();
                var str_obj1 = xmlSerializer.Serialize(manifest);
                var str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);


                //test "AddOrUPDATE"
                manifest.Status = "foo";
                uow.MultiTenantObjectRepository.AddOrUpdate(uow, userDO.Id, manifest, a => a.EnvelopeId);
                manifest_from_MTO = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);

                // test "ADDorUpdate"
                var manifest2 = FixtureData___MultiTenantObjectSubClass.TestData1();
                manifest2.EnvelopeId = "2";
                uow.MultiTenantObjectRepository.AddOrUpdate(uow, userDO.Id, manifest2, keyProperty: b => b.EnvelopeId);
                uow.SaveChanges();

                var manifest_from_MTO2 = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == "2");
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest2);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO2);
                Assert.AreEqual(str_obj1, str_obj2);


                manifest_from_MTO = null;
                //Delete test
                uow.MultiTenantObjectRepository.Remove<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                manifest_from_MTO = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);


                uow.SaveChanges();
                Assert.AreEqual(manifest_from_MTO, null);
            }
        }
    }
}
