using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Newtonsoft.Json;
using Utilities;

namespace Data.Repositories
{
    public class RemoteServiceProviderRepository : GenericRepository<RemoteServiceProviderDO>, IRemoteCalendarProviderRepository
    {
        internal RemoteServiceProviderRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteServiceProviderDO GetByName(string name)
        {
            return GetQuery().FirstOrDefault(rcp => rcp.Name == name);
        }

        public void CreateRemoteCalendarProviders(IConfigRepository configRepository)
        {
            var clientID = configRepository.Get("GoogleCalendarClientId");
            var clientSecret = configRepository.Get("GoogleCalendarClientSecret");
            var providers = new[]
                {
                    new RemoteServiceProviderDO
                        {
                            Name = "Google",
                            AuthType = ServiceAuthorizationType.OAuth2,
                            AppCreds = JsonConvert.SerializeObject(
                                new
                                    {
                                        ClientId = clientID,
                                        ClientSecret = clientSecret,
                                        Scopes = "https://www.googleapis.com/auth/calendar,email"
                                    }),
                            EndPoint = "https://apidata.googleusercontent.com/caldav/v2"
                        }
                };
            foreach (var provider in providers)
            {
                var existingRow = GetByName(provider.Name);
                if (existingRow == null)
                {
                    Add(provider);
                }
                else
                {
                    existingRow.AuthType = provider.AuthType;
                    existingRow.AppCreds = provider.AppCreds;
                    existingRow.EndPoint = provider.EndPoint;
                }
            }
        }
    }

    public interface IRemoteCalendarProviderRepository : IGenericRepository<RemoteServiceProviderDO>
    {
        RemoteServiceProviderDO GetByName(string name);
    }
}
