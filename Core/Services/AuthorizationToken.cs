using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    public class AuthorizationToken
    {
        public string GetToken(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAuthToken = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (curAuthToken != null)
                    return curAuthToken.Token;
            }
            return null;
        }

        public void AddOrUpdateToken(string userId, string token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (tokenDO == null)
                {
                    tokenDO = new AuthorizationTokenDO()
                    {
                        UserID = userId
                    };
                    uow.AuthorizationTokenRepository.Add(tokenDO);
                }
                tokenDO.ExpiresAt = DateTime.Now.AddYears(100);
                tokenDO.Token = token;
                uow.SaveChanges();
            }
        }

        public void RemoveToken(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tokenDO = uow.AuthorizationTokenRepository.FindOne(at => at.UserID == userId);
                if (tokenDO != null)
                {
                    uow.AuthorizationTokenRepository.Remove(tokenDO);
                    uow.SaveChanges();
                }
            }
        }
    }
}
