using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Workspaces : IAsanaWorkspaces
    {
        public IEnumerable<AsanaWorkspace> GetAvaliableWorkspaces()
        {
            var result = new List<AsanaWorkspace>();


            return result;
        }

        public bool UpdateWorkspace(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public AsanaWorkspace Get(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AsanaWorkspace> GetAll()
        {
            throw new NotImplementedException();
        }

        public AsanaWorkspace Update(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Search(WorkspaceSearchQuery query)
        {
            throw new NotImplementedException();
        }

        public AsanaUser AddUser(AsanaUser user)
        {
            throw new NotImplementedException();
        }

        public bool RemoveUser(AsanaUser user)
        {
            throw new NotImplementedException();
        }
    }
}