using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Users : IAsanaUsers
    {
        public Users(IRestfulServiceClient client)
        {
            
        }

        public string Token { get; set; }

        public async Task<AsanaUser> Me()
        {
            return new AsanaUser();
        }

        public async Task<AsanaUser> GetUser(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AsanaUser>> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}