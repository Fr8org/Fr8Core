using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;

namespace Data.Infrastructure.AutoMapper
{
    public class ExternalEventSubscriptionToIntConverter : ITypeConverter<IList<ExternalEventSubscriptionDO>, IList<int?>>
    {
        public IList<int?> Convert(ResolutionContext context)
        {
            var source = (IList<ExternalEventSubscriptionDO>)context.SourceValue;
            return source.Select(a => a.ExternalEvent).ToList();
        }
    }

    public class IntToExternalEventSubscriptionConverter : ITypeConverter<IList<int?>, IList<ExternalEventSubscriptionDO>>
    {
        public IList<ExternalEventSubscriptionDO> Convert(ResolutionContext context)
        {
            var source = (IList<int?>)context.SourceValue;
            if (source == null)
                return new List<ExternalEventSubscriptionDO>();

            int processTermplateId = (int)context.Options.Items["ptid"];
            return source.Select(a =>
            {
                return new ExternalEventSubscriptionDO()
                {
                    ExternalEvent = a,
                    ExternalProcessTemplateId = processTermplateId
                };
            }).ToList();
        }
    }
}
