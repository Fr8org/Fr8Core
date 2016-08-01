using PlanDirectory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace PlanDirectory.Infrastructure
{
    public class TagGenerator : ITagGenerator
    {
        /// <summary>
        /// The result of this method is a a list of ActivityTemplateTag and WebServiceTemplateTag classes
        /// For a plan, that consists of activity named "A" of a webservice "Y"
        /// and of activity named "B" of a webservice "Z"
        /// the result would be:
        /// ActivityTemplateTag: A
        /// ActivityTemplateTag: B
        /// ActivityTemplateTag: A, B
        /// WebServiceTemplateTag: Y
        /// WebServiceTemplateTag: Z
        /// WebServiceTemplateTag: Y, Z
        /// </summary>
        public async Task<TemplateTagStorage> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId)
        {
            var result = new TemplateTagStorage();

            //requesting all activity templates
            var hmacService = ObjectFactory.GetInstance<IHMACService>();
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var uri = new Uri(CloudConfigurationManager.GetSetting("HubApiBaseUrl") + "activitytemplates");
            var headers = await hmacService.GenerateHMACHeader(
                uri,
                "PlanDirectory",
                CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
                fr8AccountId,
                null
            );

            var activityCategories = await client.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(
               uri, headers: headers);

            var activityDict = activityCategories.SelectMany(a => a.Activities).ToDictionary(k => k.Id);

            //1. getting ids of used templates
            var plan = planTemplateCM.PlanContents;
            var usedActivityTemplatesIds = new HashSet<Guid>();

            if (plan.SubPlans != null)
            {
                foreach (var subplan in plan.SubPlans)
                {
                    CollectActivityTemplateIds(subplan, usedActivityTemplatesIds);
                }
            }

            if (usedActivityTemplatesIds.Count == 0)
            {
                return new TemplateTagStorage();
            }

            //2. getting used templates
            var usedActivityTemplates = usedActivityTemplatesIds.Intersect(activityDict.Keys)
                                     .Select(k => activityDict[k])
                                     .Distinct(ActivityTemplateDTO.IdComparer)
                                     .OrderBy(a => a.Name)
                                     .ToList();

            if (usedActivityTemplates.Count != usedActivityTemplatesIds.Count)
                throw new ApplicationException("Template references activity that is not registered in Hub");
            //3. adding tags for activity templates
            var activityTemplatesCombinations = GetCombinations<ActivityTemplateDTO>(usedActivityTemplates);
            activityTemplatesCombinations.ForEach(a => result.ActivityTemplateTags.Add(new ActivityTemplateTag(a)));

            //4. adding tags for webservices
            var usedWebServices = usedActivityTemplates.SelectMany(a => a.Categories).Distinct(ActivityCategoryDTO.NameComparer).OrderBy(b => b.Name).ToList();
            var webServicesCombination = GetCombinations<ActivityCategoryDTO>(usedWebServices);
            webServicesCombination.ForEach(a => result.WebServiceTemplateTags.Add(new WebServiceTemplateTag(a)));

            return result;
        }

        private void CollectActivityTemplateIds(FullSubplanDto subplan, HashSet<Guid> ids)
        {
            if (subplan.Activities == null)
            {
                return;
            }

            foreach (var activity in subplan.Activities)
            {
                CollectActivityTemplateIds(activity, ids);
            }
        }

        private void CollectActivityTemplateIds(ActivityDTO activity, HashSet<Guid> ids)
        {
            if (activity.ActivityTemplate != null && activity.ActivityTemplate.Id != Guid.Empty)
            {
                ids.Add(activity.ActivityTemplate.Id);
            }

            if (activity.ChildrenActivities == null)
            {
                return;
            }

            foreach (var child in activity.ChildrenActivities)
            {
                CollectActivityTemplateIds(child, ids);
            }
        }

        /// <summary>
        /// K-combination algorythm implementation. 
        /// For input: "A, B, C"
        /// would output:
        /// A,
        /// A,B,
        /// A,B,C,
        /// A,C,
        /// B,
        /// B,C,
        /// C,
        /// 
        /// might require optimisation if this chunk will ever become a bottleneck
        /// </summary>
        private List<List<T>> GetCombinations<T>(List<T> list)
        {
            var result = new List<List<T>>();
            double count = Math.Pow(2, list.Count);
            for (int i = 1; i <= count - 1; i++)
            {
                var row = new List<T>();
                string str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        row.Add(list[j]);
                    }
                }
                result.Add(row);
            }
            return result;
        }
    }
}