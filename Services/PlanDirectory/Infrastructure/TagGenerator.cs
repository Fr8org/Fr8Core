using PlanDirectory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8Data.Manifests;
using StructureMap;
using Fr8Infrastructure.Interfaces;
using Utilities.Configuration.Azure;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Fr8Data.DataTransferObjects;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fr8Data.DataTransferObjects.PlanTemplates;

namespace PlanDirectory.Infrastructure
{
    public class TagGenerator : ITagGenerator
    {
        /// <summary>
        /// The result of this method is a set of tags
        /// For a plan, that consists of activity named "A" of a webservice "Y"
        /// and of activity named "B" of a webservice "Z"
        /// the result would be:
        /// A
        /// A, B
        /// B
        /// Y
        /// Y, Z
        /// Z
        /// </summary>

        public async Task<List<TemplateTag>> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId)
        {
            var result = new List<TemplateTag>();

            //requesting all activity templates
            var hmacService = ObjectFactory.GetInstance<IHMACService>();
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var uri = new Uri(CloudConfigurationManager.GetSetting("HubUrl") + "/api/v1/activitytemplates");
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
            var planTemplateDTO = JsonConvert.DeserializeObject<PlanTemplateDTO>(planTemplateCM.PlanContents);
            var usedActivityTemplatesIds = planTemplateDTO.PlanNodeDescriptions.Select(a => a.ActivityDescription.ActivityTemplateId).Distinct().ToList();
            //2. getting used templates
            var usedActivityTemplates = usedActivityTemplatesIds.Intersect(activityDict.Keys)
                                     .Select(k => activityDict[k])
                                     .Distinct()
                                     .OrderBy(a => a.Name)
                                     .ToList();

            if (usedActivityTemplates.Count != usedActivityTemplatesIds.Count)
                throw new ApplicationException("Template references activity that is not registered in Hub");
            //3. adding tags for activity templates
            var activityTemplatesCombinations = GetCombinations<ActivityTemplateDTO>(usedActivityTemplates);
            activityTemplatesCombinations.ForEach(a => result.Add(new ActivityTemplateTag(a)));

            //4. adding tags for webservices
            var usedWebServices = usedActivityTemplates.Select(a => a.WebService).Distinct().OrderBy(b => b.Name).ToList();
            var webServicesCombination = GetCombinations<WebServiceDTO>(usedWebServices);
            webServicesCombination.ForEach(a => result.Add(new WebServiceTemplateTag(a)));
            
            return result;
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



    public class ActivityTemplateTag : TemplateTag
    {
        //id, WebServiceId, name
        private List<Tuple<Guid, int, string>> _values = new List<Tuple<Guid, int, string>>();

        [JsonIgnore]
        public override string Values { get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); } }

        public ActivityTemplateTag(List<ActivityTemplateDTO> values)
        {
            values.ForEach(a =>
            { _values.Add(new Tuple<Guid, int, string>(a.Id, a.WebService.Id, a.Name)); });
        }

    }

    public class WebServiceTemplateTag : TemplateTag
    {
        //id, iconPath, name
        private List<Tuple<int, string, string>> _values = new List<Tuple<int, string, string>>();

        [JsonIgnore]
        public override string Values { get { return string.Join(", ", _values.Select(a => a.Item3).ToArray()); } }

        public WebServiceTemplateTag(List<WebServiceDTO> values)
        {
            values.ForEach(a =>
            { _values.Add(new Tuple<int, string, string>(a.Id, a.IconPath, a.Name)); });
        }
    }

    public abstract class TemplateTag
    {
        [JsonIgnore]
        public abstract string Values { get; }
    }
}