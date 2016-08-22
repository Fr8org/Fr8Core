using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    public class ProfileController : Controller
    {
        [DockyardAuthorize(Roles = "Admin")]
        public ActionResult Index(int profileID)
        {
            if (profileID == 0)
                return new HttpNotFoundResult();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var profileDO = uow.ProfileRepository.GetQuery().FirstOrDefault(pn => pn.Id == profileID);
                var vm = new ProfileVM(profileDO);
                return View(vm);
            }
        }

        [DockyardAuthorize(Roles = "Admin")]
        public ActionResult ProfilesForUser(string userID)
        {
            if (String.IsNullOrWhiteSpace(userID))
                return new HttpNotFoundResult();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userID);
                var profiles = uow.ProfileRepository.GetQuery().Where(pn => pn.DockyardAccountID == userID);
                var vm = new UserProfilesVM
                {
                    UserName = userDO.UserName,
                    UserProfiles = profiles.Select(p => new UserProfileVM
                    {
                        Id = p.Id,
                        ProfileName = p.Name
                    }).ToList()
                };
                return View(vm);
            }
        }
        
        [HttpPost]
        [DockyardAuthorize(Roles = "Admin")]
        public ActionResult UpdateProfile(ProfileVM value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentIDs = value.Nodes.SelectMany(GetCurrentIDs).Where(id => id > 0).ToList();
                var rowsToDelete = uow.ProfileNodeRepository.GetQuery().Where(pn => !currentIDs.Contains(pn.Id)).ToList();
                foreach (var rowToDelete in rowsToDelete)
                {
                    foreach(var profileItem in rowToDelete.ProfileItems.ToList())
                        uow.ProfileItemRepository.Remove(profileItem);

                    uow.ProfileNodeRepository.Remove(rowToDelete);
                }

                CreateNewNodes(uow, value.Id, null, value.Nodes);
                uow.SaveChanges();
            }
            return Json(true);
        }

        [DockyardAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateNewProfile()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProfileRepository.Add(new ProfileDO {Name = "New Profile", DockyardAccountID = this.GetUserId()});
                uow.SaveChanges();
            }
            return Json(true);
        }

        [DockyardAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteProfile(int profileID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var profileDO = uow.ProfileRepository.GetByKey(profileID);
                uow.ProfileRepository.Remove(profileDO);
                uow.SaveChanges();
            }
            return Json(true);
        }

        [DockyardAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult RenameProfile(int profileID, String newUserName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var profileDO = uow.ProfileRepository.GetByKey(profileID);
                profileDO.Name = newUserName;
                uow.SaveChanges();
            }
            return Json(true);
        }

        private void CreateNewNodes(IUnitOfWork uow, int profileID, ProfileNodeDO parentProfileNodeDO, IEnumerable<ProfileVM.ProfileNodeVM> nodes)
        {
            foreach (var node in nodes)
            {
                ProfileNodeDO nodeDO;
                if (node.Id == 0)
                {
                    nodeDO = new ProfileNodeDO();
                    uow.ProfileNodeRepository.Add(nodeDO);
                }
                else
                {
                    nodeDO = uow.ProfileNodeRepository.GetByKey(node.Id);
                }
                nodeDO.ProfileID = profileID;
                nodeDO.Name = node.Label;

                //Add profile items..
                if (node.Items != null)
                {
                    HashSet<int> currentItems = new HashSet<int>();
                    foreach (var item in node.Items)
                    {
                        ProfileItemDO profileItemDO;
                        if (item.Id == 0)
                        {
                            profileItemDO = new ProfileItemDO();
                            uow.ProfileItemRepository.Add(profileItemDO);
                        }
                        else
                        {
                            profileItemDO = uow.ProfileItemRepository.GetByKey(item.Id);
                        }
                        profileItemDO.ProfileNode = nodeDO;
                        profileItemDO.Key = item.ItemName;
                        profileItemDO.Value = item.ItemValue;
                        currentItems.Add(item.Id);
                    }
                    var rowsToDelete = uow.ProfileItemRepository.GetQuery().Where(pi => !currentItems.Where(a => a != 0).Contains(pi.Id)).ToList();
                    foreach(var rowToDelete in rowsToDelete)
                        uow.ProfileItemRepository.Remove(rowToDelete);
                }
                else {
                    var rowsToDelete = uow.ProfileItemRepository.GetQuery().Where(pi => pi.ProfileNodeID == nodeDO.Id).ToList();
                    foreach (var rowToDelete in rowsToDelete)
                        uow.ProfileItemRepository.Remove(rowToDelete);
                }

                nodeDO.ParentNode = parentProfileNodeDO;

                if (node.Children != null)
                {
                    CreateNewNodes(uow, profileID, nodeDO, node.Children);
                }
            }
        }

        private IEnumerable<int> GetCurrentIDs(ProfileVM.ProfileNodeVM profileNode)
        {
            yield return profileNode.Id;
            if (profileNode.Children != null)
                foreach (var child in profileNode.Children)
                    foreach (var id in GetCurrentIDs(child))
                        yield return id;
        }
            
	}
}