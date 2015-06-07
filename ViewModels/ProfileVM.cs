using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class ProfileVM
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String UserName { get; set; }
        public String UserId { get; set; }
        public IList<ProfileNodeVM> Nodes { get; set; }

        public ProfileVM()
        {
            
        }
        public ProfileVM(ProfileDO profileDO)
        {
            Id = profileDO.Id;
            Name = profileDO.Name;
            UserName = profileDO.User.UserName;
            UserId = profileDO.UserID;
            Nodes = profileDO.ProfileNodes.Where(pn => pn.ParentNodeID == null).Select(CreateNodeVM).ToList();
        }
        private ProfileNodeVM CreateNodeVM(ProfileNodeDO profileNode)
        {
            return new ProfileNodeVM
            {
                Id = profileNode.Id,
                Label = profileNode.Name,
                Items = profileNode.ProfileItems.Select(pi => new ProfileNodeItemVM
                {
                    Id = pi.Id,
                    ItemName = pi.Key,
                    ItemValue = pi.Value
                }).ToList(),
                Children = profileNode.ChildNodes.Select(CreateNodeVM).ToList()
            };
        }

        public class ProfileNodeVM
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public IList<ProfileNodeVM> Children { get; set; }
            public IList<ProfileNodeItemVM> Items { get; set; } 
        }

        public class ProfileNodeItemVM
        {
            public int Id { get; set; }
            public String ItemName { get; set; }
            public String ItemValue { get; set; }
        }
    }
}