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
        private readonly IAsanaOAuth _oAuthService;

        public Users(IRestfulServiceClient client, IAsanaOAuth oAuth)
        {
            _restfulClient = client;
            _oAuthService = oAuth;
        }
        



        public async Task<AsanaUser> Me()
        {
            return new AsanaUser() {Name = "Test"};
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