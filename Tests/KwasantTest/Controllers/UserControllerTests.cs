using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Services;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Controllers
{
    public class UserControllerTests : BaseTest
    {
        [Test]
        public void ShowAllTestNoUsers()
        {
            //Database starts empty - we have no users
            var usersModel = GetAllUsers();

            //Check we have no users
            Assert.AreEqual(0, usersModel.Count);
        }

        [Test]
        public void ShowAllTestWithUserNoRoles()
        {
            CreateUser("rjrudman@gmail.com");

            var usersModel = GetAllUsers();
            
            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have no roles
            var userVM = usersModel.First();
            Assert.AreEqual(0, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());
        }

        [Test]
        public void ShowAllTestWithUserWithOneRole()
        {
            const string roleName = "Customer";
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            Assert.AreEqual(1, new UserController().ConvertRoleStringToRoles(usersModel.First().Role).Count());
            var firstRole = usersModel.First().Role;
            Assert.AreEqual(roleName, firstRole);
        }

        [Test]
        public void ShowAllTestWithUserWithMultipleRoles()
        {
            const string firstRoleName = Roles.Admin;
            AddAllRoles();
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, firstRoleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have all roles
            Assert.AreEqual(3, new UserController().ConvertRoleStringToRoles(usersModel.First().Role).Count());
            var firstRole = usersModel.First().Role;
            Assert.AreEqual(firstRoleName, firstRole);
        }

        [Test]
        public void TestUpdateUserBasicDetails()
        {
            CreateUser("rjrudman@gmail.com");

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);
            var firstUserModel = usersModel.First();

            Assert.IsNull(firstUserModel.FirstName);

            firstUserModel.FirstName = "Rob";
            UpdateUser(firstUserModel);

            //Now check users after updating
            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);
            firstUserModel = usersModel.First();

            Assert.AreEqual("Rob", firstUserModel.FirstName);
        }

        [Test]
        public void TestUpdateUserRoleAdd()
        {
            const string roleName = "Customer";
            CreateUser("rjrudman@gmail.com");
            CreateRole(roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check he has no roles
            var userVM = usersModel.First();
            Assert.AreEqual(0, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());

            userVM.Role = roleName;
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            Assert.AreEqual(1, new UserController().ConvertRoleStringToRoles(usersModel.First().Role).Count());
            var firstRole = usersModel.First().Role;
            Assert.AreEqual(roleName, firstRole);
        }

        [Test]
        public void TestUpdateUserRoleRemove()
        {
            const string roleName = "Admin";
            AddAllRoles();
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have all role
            var userVM = usersModel.First();

            Assert.AreEqual(3, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());
            var firstRole = userVM.Role;
            Assert.AreEqual(roleName, firstRole);

            userVM.Role = "";
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have no roles
            Assert.AreEqual(0, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());
        }

        [Test]
        public void TestUpdateUserRoleAddAndRemove()
        {
            const string firstRoleName = Roles.Admin;
            const string secondRoleName = Roles.Booker;
            AddAllRoles();

            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, firstRoleName);
            //CreateRole(secondRoleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have all role
            var userVM = usersModel.First();
            Assert.AreEqual(3, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());
            var firstRole = userVM.Role;
            Assert.AreEqual(firstRoleName, firstRole);

            userVM.Role = secondRoleName;
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly two role
            Assert.AreEqual(2, new UserController().ConvertRoleStringToRoles(userVM.Role).Count());
        }

        [Test]
        public void TestDetail()
        {
            UserDO userDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user
                userDO = uow.UserRepository.GetOrCreateUser("rjrudman@gmail.com");
                uow.SaveChanges();
            }

            var controller = new UserController();
            //Check we get a view back
            var res = controller.Details(userDO.Id) as ViewResult;
            Assert.NotNull(res);
        }

        private void UpdateUser(UserVM userVM)
        {
            var controller = new UserController();
            //Check we get a view back
            var res = controller.ProcessAddUser(userVM);
            Assert.NotNull(res);
        }

        private static List<UserVM> GetAllUsers()
        {
            var controller = new UserController();
            //Check we get a view back
            var res = controller.Index() as ViewResult;
            Assert.NotNull(res);

            //Check the view has a model
            var model = res.Model as List<UserVM>;
            Assert.NotNull(model);

            return model;
        }

        private static UserDO CreateUser(String emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user, do not give him roles
                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);
                uow.SaveChanges();

                return userDO;
            }
        }

        private static void CreateRole(String roleID, String roleName = null)
        {
            if (roleName == null)
                roleName = roleID;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!uow.AspNetRolesRepository.GetQuery().Any(r => r.Name == roleID))
                {
                    //Create a role
                    uow.AspNetRolesRepository.Add(new AspNetRolesDO {Id = roleID, Name = roleName});
                    uow.SaveChanges();
                }
            }
        }

        private static void AssignRoleToUser(UserDO userDO, String roleID, String roleName = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateRole(roleID, roleName);
                //Assign user to role
                uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
                uow.SaveChanges();
            }
        }

        [Test]
        public void TestCanDeleteAndChangeUserStatus()
        {
            UserDO userDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //SETUP
                //Create a active user
                userDO = uow.UserRepository.GetOrCreateUser("test.user@gmail.com");
                userDO.State = UserState.Active;
                uow.SaveChanges();
                var controller = new UserController();

                //EXECUTE
                //Calling controller action
                controller.UpdateStatus(userDO.Id, UserState.Deleted);
                
                //VERIFY
                Assert.AreEqual(1, uow.UserRepository.GetQuery().Where(e => e.State == UserState.Deleted).Count());
                
                controller.UpdateStatus(userDO.Id, UserState.Suspended);
                Assert.AreEqual(1, uow.UserRepository.GetQuery().Where(e => e.State == UserState.Suspended).Count());
            }
        }

        private void AddAllRoles() 
        {
            const string firstRoleName = Roles.Admin;
            const string secondRoleName = Roles.Booker;
            const string thirdRoleName = Roles.Customer;
            CreateRole(firstRoleName);
            CreateRole(secondRoleName);
            CreateRole(thirdRoleName);
        }
    }
}
