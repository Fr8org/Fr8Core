using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;

namespace Data.Infrastructure.AutoMapper
{
    public class ExternalEventSubscriptionToStringConverter : ITypeConverter<IList<ExternalEventSubscriptionDO>, IList<string>>
    {
        public IList<string> Convert(ResolutionContext context)
        {
            var source = (IList<ExternalEventSubscriptionDO>)context.SourceValue;
            return source.Select(a => a.ExternalEvent.ToString()).ToList();
        }
    }

    public class StringToExternalEventSubscriptionConverter : ITypeConverter<IList<string>, IList<ExternalEventSubscriptionDO>>
    {
        public IList<ExternalEventSubscriptionDO> Convert(ResolutionContext context)
        {
            var source = (IList<string>)context.SourceValue;
            int processTermplateId = System.Convert.ToInt32(context.Options.Items["ptid"]);
            return source.Select(a =>
            {
                return new ExternalEventSubscriptionDO()
                {
                    ExternalEvent = int.Parse(a),
                    ProcessTemplateId = processTermplateId
                };
            }).ToList();
        }
    }
}
