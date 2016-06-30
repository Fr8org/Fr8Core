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
        private readonly IRestfulServiceClient _restfulClient;
        private readonly IAsanaParameters _asanaParams;

        public Users(IRestfulServiceClient client, IAsanaParameters asanaParams)
        {
            _restfulClient = client;
            _asanaParams = asanaParams;
        }

        public async Task<AsanaUser> Me()
        {

            return new AsanaUser() {Name = "Test"};
        }

        public async Task<AsanaUser> GetUser(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AsanaUser>> GetUsers(string  workspaceId)
        {

            throw new NotImplementedException();
        }
    }
}