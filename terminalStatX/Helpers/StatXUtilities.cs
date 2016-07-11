using System;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;

namespace terminalStatX.Helpers
{
    public class StatXUtilities
    {
        private const string AdvisoryName = "StatX Warning";
        private const string AdvisoryContent = "Your selected stat is missing Title. Please insert Title value inside your mobile app for this stat, so you have understandable name.";


        public static StatXItemCM MapToStatItemCrateManifest(StatDTO stat)
        {
            var result = new StatXItemCM()
            {
                Id = stat.Id,
                Title = stat.Title,
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

        public static bool CompareStatsForValueChanges(StatXItemCM oldStat, StatXItemCM newStat)
        {
            if (DateTime.Parse(oldStat.LastUpdatedDateTime) >= DateTime.Parse(newStat.LastUpdatedDateTime))
                return false;

            if (newStat.StatValueItems.Any())
            {
                foreach (var item in newStat.StatValueItems)
                {
                    var oldStatItem = oldStat.StatValueItems.FirstOrDefault(x => x.Name == item.Name);
                    if (oldStatItem != null && item.Value != oldStatItem.Value)
                    {
                        return true;
                    }
                }

                return true;
            }

            return oldStat.Value != newStat.Value;
        }

        public static StatXAuthDTO GetStatXAuthToken(string token)
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(token);
        }
        public static void AddAdvisoryMessage(ICrateStorage storage)
        {
            var advisoryCrate = storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault();
            var currentAdvisoryResults = advisoryCrate == null ? new AdvisoryMessagesCM() : advisoryCrate.Content;

            var advisory = currentAdvisoryResults.Advisories.FirstOrDefault(x => x.Name == AdvisoryName);

            if (advisory == null)
            {
                currentAdvisoryResults.Advisories.Add(new AdvisoryMessageDTO { Name = AdvisoryName, Content = AdvisoryContent });
            }
            else
            {
                advisory.Content = AdvisoryContent;
            }

            storage.Add(Crate.FromContent("Advisories", currentAdvisoryResults));
        }


        public static StatXAuthDTO GetStatXAuthToken(AuthorizationToken token)
        {
            return JsonConvert.DeserializeObject<StatXAuthDTO>(token.Token);
        }
    }
}