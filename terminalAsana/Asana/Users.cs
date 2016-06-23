using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalAsana.Asana.Entites;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Users : IAsanaUsers
    {
        public AsanaUser Me()
        {
            throw new NotImplementedException();
        }

        public AsanaUser GetUser(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AsanaUser> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}