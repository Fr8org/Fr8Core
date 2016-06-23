using System.Linq;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;

namespace terminalStatX.Helpers
{
    public class StatXUtilities
    {
        public static StatXItemCM MapToStatItemCrateManifest(StatDTO stat)
        {
            var result = new StatXItemCM()
            {
                Id = stat.Id,
                Value = stat.Value,
                LastUpdatedDateTime = stat.LastUpdatedDateTime,
                StatValueItems = stat.StatItems.Select(x => new StatValueItemDTO()
                {
                    Name = x.Name,
                    Value = x.Value
                }).ToList()
            };

            return result;
        }

        public static StatXAuthDTO GetStatXAuthToken(AuthorizationToken token)
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(token.Token);
        }
    }
}