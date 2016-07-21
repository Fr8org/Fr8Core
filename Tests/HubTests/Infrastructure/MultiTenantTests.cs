using System.Collections.Generic;
using System.Linq;
using RestSharp.Serializers;
using NUnit.Framework;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Infrastructure
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
                uow.MultiTenantObjectRepository.Add(manifest, userDO.Id);
                uow.SaveChanges();

                //test "Update()"
                manifest.Status = "newstatus";
                uow.MultiTenantObjectRepository.Update(userDO.Id, manifest);
                uow.SaveChanges();

                var manifest_from_MTO = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId).FirstOrDefault();
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
                uow.MultiTenantObjectRepository.Add( manifest, userDO.Id);
                uow.SaveChanges();
                
                
                //test "AddOrUPDATE"
                manifest.Status = "foo";
                uow.MultiTenantObjectRepository.AddOrUpdate(userDO.Id, manifest);
                uow.SaveChanges();

                var manifest_from_MTO = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId).FirstOrDefault();
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
                uow.MultiTenantObjectRepository.Add(manifest, userDO.Id);
                uow.SaveChanges();

                //test "Update()"
                manifest.Status = "newstatus";
                uow.MultiTenantObjectRepository.Update(userDO.Id, manifest);
                uow.SaveChanges();

                var manifest_from_MTO = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId).FirstOrDefault();
                XmlSerializer xmlSerializer = new XmlSerializer();
                var str_obj1 = xmlSerializer.Serialize(manifest);
                var str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);


                //test "AddOrUPDATE"
                manifest.Status = "foo";
                uow.MultiTenantObjectRepository.AddOrUpdate(userDO.Id, manifest);
                uow.SaveChanges();
                manifest_from_MTO = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId).FirstOrDefault();
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);

                // test "ADDorUpdate"
                var manifest2 = FixtureData___MultiTenantObjectSubClass.TestData1();
                manifest2.EnvelopeId = "2";
                uow.MultiTenantObjectRepository.AddOrUpdate(userDO.Id, manifest2);
                uow.SaveChanges();

                var manifest_from_MTO2 = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == "2").FirstOrDefault();
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest2);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO2);
                Assert.AreEqual(str_obj1, str_obj2);


               
                //Delete test
                uow.MultiTenantObjectRepository.Delete<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId);
                uow.SaveChanges();
                manifest_from_MTO = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>(userDO.Id, a => a.EnvelopeId == manifest.EnvelopeId).FirstOrDefault();


                Assert.AreEqual(manifest_from_MTO, null);
            }
        }

        [Test]
        public void MTO_Test_CRUD_NonPrimitiveProperties()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                //adding object to repo
                var manifest = FixtureData___MultiTenantObjectSubClass.TestData2();
                uow.MultiTenantObjectRepository.Add(manifest, userDO.Id);
                uow.SaveChanges();

                //test "Update()"
                manifest.PayloadObjects.Add(
                    new PayloadObjectDTO()
                    {
                        PayloadObject = new List<KeyValueDTO>()
                        {
                            new KeyValueDTO()
                            {
                                Key = "Key2",
                                Value = "Value2"
                            }
                        }
                    }
                );

                uow.MultiTenantObjectRepository.Update(userDO.Id, manifest, b => b.ObjectType == manifest.ObjectType);
                uow.SaveChanges();

                var manifest_from_MTO = uow.MultiTenantObjectRepository.Query<StandardPayloadDataCM>(userDO.Id, a => a.ObjectType == manifest.ObjectType).FirstOrDefault();
                XmlSerializer xmlSerializer = new XmlSerializer();
                var str_obj1 = xmlSerializer.Serialize(manifest);
                var str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);


                //test "AddOrUPDATE"
                manifest.PayloadObjects.Add(
                    new PayloadObjectDTO()
                    {
                        PayloadObject = new List<KeyValueDTO>()
                        {
                            new KeyValueDTO()
                            {
                                Key = "Key3",
                                Value = "Value3"
                            }
                        }
                    }
                );
                uow.MultiTenantObjectRepository.AddOrUpdate(userDO.Id, manifest, a => a.ObjectType == manifest.ObjectType);
                uow.SaveChanges();
                manifest_from_MTO = uow.MultiTenantObjectRepository.Query<StandardPayloadDataCM>(userDO.Id, a => a.ObjectType == manifest.ObjectType).FirstOrDefault();
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO);
                Assert.AreEqual(str_obj1, str_obj2);

                // test "ADDorUpdate"
                var manifest2 = FixtureData___MultiTenantObjectSubClass.TestData2();
                manifest2.ObjectType = "ObjectType2";
                uow.MultiTenantObjectRepository.AddOrUpdate(userDO.Id, manifest2, b => b.ObjectType == manifest2.ObjectType);
                uow.SaveChanges();

                var manifest_from_MTO2 = uow.MultiTenantObjectRepository.Query<StandardPayloadDataCM>(userDO.Id, a => a.ObjectType == "ObjectType2").FirstOrDefault();
                xmlSerializer = new XmlSerializer();
                str_obj1 = xmlSerializer.Serialize(manifest2);
                str_obj2 = xmlSerializer.Serialize(manifest_from_MTO2);
                Assert.AreEqual(str_obj1, str_obj2);


                manifest_from_MTO = null;
                //Delete test
                uow.MultiTenantObjectRepository.Delete<StandardPayloadDataCM>(userDO.Id, a => a.ObjectType == manifest.ObjectType);
                uow.SaveChanges();
                manifest_from_MTO = uow.MultiTenantObjectRepository.Query<StandardPayloadDataCM>(userDO.Id, a => a.ObjectType == manifest.ObjectType).FirstOrDefault();


                uow.SaveChanges();
                Assert.AreEqual(manifest_from_MTO, null);
            }
        }
    }
}
