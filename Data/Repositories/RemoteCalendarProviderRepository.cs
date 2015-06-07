using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Newtonsoft.Json;
using Utilities;

namespace Data.Repositories
{
    public class RemoteCalendarProviderRepository : GenericRepository<RemoteCalendarProviderDO>, IRemoteCalendarProviderRepository
    {
        internal RemoteCalendarProviderRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteCalendarProviderDO GetByName(string name)
        {
            return GetQuery().FirstOrDefault(rcp => rcp.Name == name);
        }

        public void CreateRemoteCalendarProviders(IConfigRepository configRepository)
        {
            var clientID = configRepository.Get("GoogleCalendarClientId");
            var clientSecret = configRepository.Get("GoogleCalendarClientSecret");
            var providers = new[]
                {
                    new RemoteCalendarProviderDO
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
                            CalDAVEndPoint = "https://apidata.googleusercontent.com/caldav/v2"
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
                    existingRow.CalDAVEndPoint = provider.CalDAVEndPoint;
                }
            }
        }
    }

    public interface IRemoteCalendarProviderRepository : IGenericRepository<RemoteCalendarProviderDO>
    {
        RemoteCalendarProviderDO GetByName(string name);
    }
}
