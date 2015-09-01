using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;

namespace Data.Infrastructure.AutoMapper
{
    public class DocuSignTemplateSubscriptionToStringConverter : ITypeConverter<IList<DocuSignTemplateSubscriptionDO>, IList<string>>
    {
        public IList<string> Convert(ResolutionContext context)
        {
            var source = (IList<DocuSignTemplateSubscriptionDO>)context.SourceValue;
            return source.Select(a => a.DocuSignTemplateId).ToList<string>();
        }
    }

    public class StringToDocuSignTemplateSubscriptionConverter : ITypeConverter<IList<string>, IList<DocuSignTemplateSubscriptionDO>>
    {
        public IList<DocuSignTemplateSubscriptionDO> Convert(ResolutionContext context)
        {
            var source = (IList<string>)context.SourceValue;
            if (source == null)
                return new List<DocuSignTemplateSubscriptionDO>();

            int processTermplateId = System.Convert.ToInt32(context.Options.Items["ptid"]);
            return source.Select(a =>
            {
                return new DocuSignTemplateSubscriptionDO()
                {
                    DocuSignTemplateId = a,
                    ExternalProcessTemplateId = processTermplateId
                };
            }).ToList();
        }
    }
}
