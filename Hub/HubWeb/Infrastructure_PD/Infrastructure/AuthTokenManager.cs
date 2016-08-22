//using System;
//using System.Runtime.Caching;

//namespace HubWeb.Infrastructure_PD.Infrastructure
//{
//    public class AuthTokenManager : IAuthTokenManager
//    {
//        private const string TokenToFr8AccountPrefix = "TokenToFr8Account_";


//        public string CreateToken(Guid fr8AccountId)
//        {
//            var token = Guid.NewGuid().ToString();
//            MemoryCache.Default.Add(
//                TokenToFr8AccountPrefix + token,
//                fr8AccountId.ToString(),
//                DateTimeOffset.Now.AddMinutes(10)
//            );

//            return token;
//        }

//        public Guid? GetFr8AccountId(string token)
//        {
//            var fr8AccountId = (string)MemoryCache.Default
//                .Get(TokenToFr8AccountPrefix + token);

//            if (string.IsNullOrEmpty(fr8AccountId))
//            {
//                return null;
//            }

//            return Guid.Parse(fr8AccountId);
//        }
//    }
//}