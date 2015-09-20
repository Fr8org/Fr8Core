using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    public class AuthData
    {
        public void SetUserAuthData(string userId, string providerName, string authData)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserAuthData = uow.RemoteServiceAuthDataRepository.GetOrCreate(userId, providerName);
                curUserAuthData.Token = authData;
                uow.SaveChanges();
            }
        }

        public string GetUserAuthData(string userId, string providerName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserAuthData = uow.RemoteServiceAuthDataRepository.GetOrCreate(userId, providerName);
                return curUserAuthData.Token;
            }
        }
    }
}
