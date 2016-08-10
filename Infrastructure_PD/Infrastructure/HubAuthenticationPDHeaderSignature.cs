//using System.Net.Http;
//using Fr8.Infrastructure.Interfaces;

//namespace PlanDirectory.Infrastructure
//{
//    public class HubAuthenticationPDHeaderSignature : IRequestSignature
//    {
//        private readonly string _fr8Token;
//        public HubAuthenticationPDHeaderSignature(string token, string userId)
//        {
//            _fr8Token = $"key={token}" + (string.IsNullOrEmpty(userId) ? "" : $", user={userId}");
//        }

//        public void SignRequest(HttpRequestMessage request)
//        {
//            request.Headers.Add(System.Net.HttpRequestHeader.Authorization.ToString(), $"FR8-PD {_fr8Token}");
//        }
//    }
//}