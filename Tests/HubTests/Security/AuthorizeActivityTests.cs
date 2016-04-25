using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Castle.DynamicProxy;
using Data.Entities;
using Data.Infrastructure.Security;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Repositories.Security;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Castle.DynamicProxy;
using Hub.Security;

namespace HubTests.Security
{
    [TestFixture]
    [Category("Authorization")]
    public class AuthorizeActivityTests : BaseTest
    {
        private ISecurityServices _securityServices;
        private IActivity _activity;
        private ISecurityObjectsStorageProvider _objectsStorageProvider;
        private Guid readRolePrivilegeId;
        private Guid editRolePrivilegeID;


        private CustomDataObject customDataObject { get; set; }
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _objectsStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();

        }

        public class CustomDataObject : BaseObject
        {
            public CustomDataObject()
            {
            }

            [Key]
            public Guid Id { get; set; }

            [AuthorizeActivity(Privilege = Privilege.EditObject)]
            public virtual string Name
            {
                get; set;
            }
        }

        [Test, ExpectedException(typeof(HttpException), ExpectedMessage = "You are not authorized to perform this activity!")]
        public void AuthorizeActivityByPrivilegeOnProperty()
        {
            var proxyGenerator = new ProxyGenerator();
            customDataObject = proxyGenerator.CreateClassProxy<CustomDataObject>(new AuthorizeActivityInterceptor());

            customDataObject.Id = Guid.NewGuid();
            //Create rolePrivilegeFor this
            _objectsStorageProvider.InsertObjectRolePrivilege(customDataObject.Id.ToString(), readRolePrivilegeId,
                "CustomDataObject", "Name");
            //set should pass because of editprivilege
            customDataObject.Name = "TestValue";
            //get should raise an error
            Assert.AreEqual("TestValue", customDataObject.Name);
        }

        [Test]
        public async void CanSetDefaultObjectSecurity()
        {
            ActivityDO origActivityDO;

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();
                uow.PlanRepository.Add(plan);

                var subPlane = FixtureData.TestSubPlanDO1();
                plan.ChildNodes.Add(subPlane);

                origActivityDO = new FixtureData(uow).TestActivity3();

                origActivityDO.ParentPlanNodeId = subPlane.Id;

                uow.ActivityTemplateRepository.Add(origActivityDO.ActivityTemplate);
                uow.SaveChanges();
            }

            IActivity activity = new Activity(ObjectFactory.GetInstance<ICrateManager>(), ObjectFactory.GetInstance<IAuthorization>(), ObjectFactory.GetInstance<ISecurityServices>(), ObjectFactory.GetInstance<IActivityTemplate>(), ObjectFactory.GetInstance<IPlanNode>());

            //here because we create new object default security is set
            //Add
            await activity.SaveOrUpdateActivity(origActivityDO);

            ActivityDO activityDO;

            //Get
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //get by Id is decorated with activity attribute, so because of default security it should return us without exception
                activityDO = activity.GetById(uow, origActivityDO.Id);
            }

            Assert.AreEqual(origActivityDO.Id, activityDO.Id);

            ISubPlan subPlan = new SubPlan();
            //Delete
            await subPlan.DeleteActivity(null, activityDO.Id, true);

            var objRolePrivilege =_objectsStorageProvider.GetRolePrivilegesForSecuredObject(origActivityDO.Id.ToString());

            //check if this object role privileges has all standard role privieles
            Assert.IsNotNull(objRolePrivilege.RolePrivileges.FirstOrDefault(x=>x.Privilege.Name == Privilege.ReadObject.ToString()));
            Assert.IsNotNull(objRolePrivilege.RolePrivileges.FirstOrDefault(x => x.Privilege.Name == Privilege.EditObject.ToString()));
            Assert.IsNotNull(objRolePrivilege.RolePrivileges.FirstOrDefault(x => x.Privilege.Name == Privilege.DeleteObject.ToString()));

        }
    }
}
