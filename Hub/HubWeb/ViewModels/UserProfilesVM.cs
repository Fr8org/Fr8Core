using System;
using System.Collections.Generic;

namespace HubWeb.ViewModels
{
    public class UserProfilesVM
    {
        public String UserName { get; set; }
        public List<UserProfileVM> UserProfiles { get; set; }
    }

    public class UserProfileVM
    {
        public int Id { get; set; }
        public String ProfileName { get; set; }
    }

}