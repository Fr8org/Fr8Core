using System.Web;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities.UAParser;

namespace Data.Repositories
{
    public class UserAgentInfoRepository : GenericRepository<UserAgentInfoDO>, IUserAgentInfoRepository
    {
        internal UserAgentInfoRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public void TrackRequest(string userID, HttpRequest request)
        {
            var uaParser = Parser.GetDefault();
            var result = uaParser.Parse(request.UserAgent);

            var userAgentInfo = new UserAgentInfoDO
            {
                UserID = userID,
                RequestingURL = request.Path,

                DeviceFamily = result.Device.Family,
                DeviceIsSpider = result.Device.IsSpider,

                OSFamily = result.OS.Family,
                OSMajor = result.OS.Major,
                OSMinor = result.OS.Minor,
                OSPatch = result.OS.Patch,
                OSPatchMinor = result.OS.Patch,

                AgentFamily = result.UserAgent.Family,
                AgentMajor = result.UserAgent.Major,
                AgentMinor = result.UserAgent.Minor,
                AgentPatch = result.UserAgent.Patch,
            };

            Add(userAgentInfo);
        }
    }


    public interface IUserAgentInfoRepository : IGenericRepository<UserAgentInfoDO>
    {

    }
}
