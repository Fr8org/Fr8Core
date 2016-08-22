using Data.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HubTests.Controllers
{
   public class UserApiControlle:ApiController
    {
       public List<AccessLevel> Get(int id)
       {
          return new List<AccessLevel>().ToList();
       }
    }
}
